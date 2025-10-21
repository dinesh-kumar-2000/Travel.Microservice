using Dapper;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using SharedKernel.Data;
using SharedKernel.Caching;
using Microsoft.Extensions.Logging;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Repository for payment operations with caching support
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class PaymentRepository : TenantBaseRepository<Payment, string>, IPaymentRepository
{
    private readonly ICacheService _cache;
    
    protected override string TableName => "payments";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public PaymentRepository(
        IDapperContext context,
        ICacheService cache,
        ILogger<PaymentRepository> logger) 
        : base(context, logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    #region Overridden Methods with Caching

    public override async Task<Payment?> GetByIdAsync(string id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        var cacheKey = $"payment:{tenantId}:{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM payments 
                WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new 
            { 
                Id = id, 
                TenantId = tenantId 
            });
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public override async Task<string> AddAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

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
            CustomerId = string.Empty,
            entity.IsDeleted,
            entity.CreatedAt
        });
        
        _logger.LogInformation("Payment {PaymentId} created for booking {BookingId}", entity.Id, entity.BookingId);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE payments 
            SET status = @Status,
                transaction_id = @TransactionId,
                provider_reference = @ProviderReference,
                completed_at = @CompletedAt,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.TenantId,
            Status = (int)entity.Status,
            entity.TransactionId,
            entity.ProviderReference,
            entity.CompletedAt
        });
        
        if (rowsAffected > 0)
        {
            // Invalidate cache
            await _cache.RemoveAsync($"payment:{entity.TenantId}:{entity.Id}", cancellationToken);
            _logger.LogInformation("Payment {PaymentId} updated to status {Status}", entity.Id, entity.Status);
        }

        return rowsAffected > 0;
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE payments 
            SET is_deleted = true, 
                deleted_at = CURRENT_TIMESTAMP 
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Payment {PaymentId} deleted (soft delete)", id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<IEnumerable<Payment>> GetByBookingIdAsync(string bookingId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bookingId))
            throw new ArgumentNullException(nameof(bookingId));

        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM payments 
            WHERE booking_id = @BookingId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Payment>(sql, new { BookingId = bookingId });
    }

    public async Task<decimal> CalculateRefundAmountAsync(string paymentId, int daysBeforeTravel, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(paymentId))
            throw new ArgumentNullException(nameof(paymentId));
        if (daysBeforeTravel < 0)
            throw new ArgumentException("Days before travel cannot be negative", nameof(daysBeforeTravel));

        using var connection = CreateConnection();
        
        // Use database function for refund calculation
        const string sql = "SELECT fn_calculate_refund_amount(@PaymentId, @DaysBeforeTravel)";
        
        var refundAmount = await connection.ExecuteScalarAsync<decimal>(sql, new 
        { 
            PaymentId = paymentId, 
            DaysBeforeTravel = daysBeforeTravel 
        });
        
        _logger.LogDebug("Calculated refund amount {Amount} for payment {PaymentId} with {Days} days before travel", 
            refundAmount, paymentId, daysBeforeTravel);
        
        return refundAmount;
    }

    public async Task<IEnumerable<Payment>> GetByTenantAndDateAsync(
        string tenantId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

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

    #endregion
}
