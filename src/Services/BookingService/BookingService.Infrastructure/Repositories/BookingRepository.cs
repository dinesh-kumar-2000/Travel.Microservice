using Dapper;
using BookingService.Domain.Entities;
using BookingService.Domain.Repositories;
using SharedKernel.Caching;
using System.Data;
using Npgsql;
using Tenancy;

namespace BookingService.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;

    public BookingRepository(string connectionString, ITenantContext tenantContext, ICacheService cache)
    {
        _connectionString = connectionString;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Booking?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"booking:{_tenantContext.TenantId}:{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM bookings 
                WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

            return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new 
            { 
                Id = id, 
                TenantId = _tenantContext.TenantId 
            });
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Booking>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        // Generate booking reference using database function if not set
        string bookingReference = entity.BookingReference;
        if (string.IsNullOrEmpty(bookingReference))
        {
            bookingReference = await connection.ExecuteScalarAsync<string>("SELECT fn_generate_booking_reference()") 
                ?? throw new InvalidOperationException("Failed to generate booking reference");
        }
        
        const string sql = @"
            INSERT INTO bookings (
                id, tenant_id, customer_id, package_id, booking_reference,
                booking_date, travel_date, number_of_travelers, total_amount,
                currency, status, idempotency_key, is_deleted, created_at
            )
            VALUES (
                @Id, @TenantId, @CustomerId, @PackageId, @BookingReference,
                @BookingDate, @TravelDate, @NumberOfTravelers, @TotalAmount,
                @Currency, @Status, @IdempotencyKey, @IsDeleted, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.TenantId,
            entity.CustomerId,
            entity.PackageId,
            BookingReference = bookingReference,
            entity.BookingDate,
            entity.TravelDate,
            entity.NumberOfTravelers,
            entity.TotalAmount,
            entity.Currency,
            Status = (int)entity.Status,
            entity.IdempotencyKey,
            entity.IsDeleted,
            entity.CreatedAt
        });
        
        return entity.Id;
    }

    public async Task UpdateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE bookings 
            SET status = @Status,
                payment_id = @PaymentId,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, entity);
        
        // Invalidate cache
        await _cache.RemoveAsync($"booking:{entity.TenantId}:{entity.Id}", cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE bookings 
            SET is_deleted = true, 
                deleted_at = @DeletedAt 
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId,
            DeletedAt = DateTime.UtcNow 
        });
        
        await _cache.RemoveAsync($"booking:{_tenantContext.TenantId}:{id}", cancellationToken);
    }

    public async Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE idempotency_key = @IdempotencyKey AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { IdempotencyKey = idempotencyKey });
    }

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId, string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE customer_id = @CustomerId 
            AND tenant_id = @TenantId 
            AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Booking>(sql, new { CustomerId = customerId, TenantId = tenantId });
    }

    public async Task<(IEnumerable<Booking> Bookings, int TotalCount)> GetPagedBookingsAsync(
        string tenantId,
        string? customerId = null,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_deleted = false" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        
        if (!string.IsNullOrEmpty(customerId))
        {
            whereClauses.Add("customer_id = @CustomerId");
            parameters.Add("CustomerId", customerId);
        }
        
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var statusEnum))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", (int)statusEnum);
        }
        
        var whereClause = string.Join(" AND ", whereClauses);
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM bookings WHERE {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        
        // Get paged data
        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT * FROM bookings 
            WHERE {whereClause}
            ORDER BY created_at DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var bookings = await connection.QueryAsync<Booking>(dataSql, parameters);
        
        return (bookings, totalCount);
    }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE booking_reference = @BookingReference 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { BookingReference = bookingReference });
    }
}

