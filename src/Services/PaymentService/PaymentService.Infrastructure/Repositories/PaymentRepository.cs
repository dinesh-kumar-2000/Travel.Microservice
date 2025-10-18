using Dapper;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using SharedKernel.Caching;
using System.Data;
using Npgsql;
using Tenancy;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;

    public PaymentRepository(string connectionString, ITenantContext tenantContext, ICacheService cache)
    {
        _connectionString = connectionString;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Payment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"payment:{_tenantContext.TenantId}:{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM payments 
                WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new 
            { 
                Id = id, 
                TenantId = _tenantContext.TenantId 
            });
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM payments 
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Payment>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO payments (
                id, tenant_id, booking_id, amount, currency, payment_method,
                status, customer_id, is_deleted, created_at
            )
            VALUES (
                @Id, @TenantId, @BookingId, @Amount, @Currency, @PaymentMethod,
                @Status, @CustomerId, @IsDeleted, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.TenantId,
            entity.BookingId,
            entity.Amount,
            entity.Currency,
            PaymentMethod = entity.PaymentMethod,
            Status = (int)entity.Status,
            CustomerId = string.Empty, // Would come from payment request
            entity.IsDeleted,
            entity.CreatedAt
        });
        
        return entity.Id;
    }

    public async Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE payments 
            SET status = @Status,
                transaction_id = @TransactionId,
                provider_reference = @ProviderReference,
                completed_at = @CompletedAt,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.TenantId,
            Status = (int)entity.Status,
            entity.TransactionId,
            entity.ProviderReference,
            entity.CompletedAt
        });
        
        // Invalidate cache
        await _cache.RemoveAsync($"payment:{entity.TenantId}:{entity.Id}", cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE payments 
            SET is_deleted = true, 
                deleted_at = CURRENT_TIMESTAMP 
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId
        });
        
        await _cache.RemoveAsync($"payment:{_tenantContext.TenantId}:{id}", cancellationToken);
    }

    public async Task<Payment?> GetByBookingIdAsync(string bookingId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM payments 
            WHERE booking_id = @BookingId AND tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC
            LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new 
        { 
            BookingId = bookingId, 
            TenantId = _tenantContext.TenantId 
        });
    }

    public async Task<decimal> CalculateRefundAmountAsync(string paymentId, int daysBeforeTravel, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        // Use database function for refund calculation
        const string sql = "SELECT fn_calculate_refund_amount(@PaymentId, @DaysBeforeTravel)";
        
        var refundAmount = await connection.ExecuteScalarAsync<decimal>(sql, new 
        { 
            PaymentId = paymentId, 
            DaysBeforeTravel = daysBeforeTravel 
        });
        
        return refundAmount;
    }

    public async Task<IEnumerable<Payment>> GetByTenantAndDateAsync(string tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM payments 
            WHERE tenant_id = @TenantId 
            AND created_at BETWEEN @StartDate AND @EndDate
            AND status = 2  -- Completed
            AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Payment>(sql, new 
        { 
            TenantId = tenantId,
            StartDate = startDate,
            EndDate = endDate
        });
    }
}

