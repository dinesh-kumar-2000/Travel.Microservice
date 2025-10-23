using Dapper;
using BookingService.Domain.Entities;
using BookingService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Caching;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Repositories;

/// <summary>
/// Repository for booking operations with caching support
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class BookingRepository : TenantBaseRepository<Booking, string>, IBookingRepository
{
    private readonly ICacheService _cache;
    
    protected override string TableName => "bookings";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public BookingRepository(
        IDapperContext context,
        ICacheService cache,
        ILogger<BookingRepository> logger) 
        : base(context, logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    #region Overridden Methods with Caching

    public override async Task<Booking?> GetByIdAsync(string id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        var cacheKey = $"booking:{tenantId}:{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM bookings 
                WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

            return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new 
            { 
                Id = id, 
                TenantId = tenantId 
            });
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public override async Task<string> AddAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

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
        
        _logger.LogInformation("Booking {BookingId} created with reference {Reference}", entity.Id, bookingReference);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE bookings 
            SET status = @Status,
                payment_id = @PaymentId,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        
        if (rowsAffected > 0)
        {
            // Invalidate cache
            await _cache.RemoveAsync($"booking:{entity.TenantId}:{entity.Id}", cancellationToken);
            _logger.LogInformation("Booking {BookingId} updated", entity.Id);
        }

        return rowsAffected > 0;
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE bookings 
            SET is_deleted = true, 
                deleted_at = @DeletedAt 
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id,
            DeletedAt = DateTime.UtcNow 
        });
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Booking {BookingId} deleted (soft delete)", id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(idempotencyKey))
            throw new ArgumentNullException(nameof(idempotencyKey));

        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE idempotency_key = @IdempotencyKey AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { IdempotencyKey = idempotencyKey });
    }

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentNullException(nameof(customerId));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

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
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(page));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));

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
        
        _logger.LogDebug("Retrieved {Count} bookings for tenant {TenantId} (page {Page})", bookings.Count(), tenantId, page);
        
        return (bookings, totalCount);
    }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bookingReference))
            throw new ArgumentNullException(nameof(bookingReference));

        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM bookings 
            WHERE booking_reference = @BookingReference 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { BookingReference = bookingReference });
    }

    #endregion
}
