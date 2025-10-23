using Dapper;
using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyTransactionRepository : TenantBaseRepository<LoyaltyTransaction, string>, ILoyaltyTransactionRepository
{
    protected override string TableName => "loyalty_transactions";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public LoyaltyTransactionRepository(IDapperContext context, ILogger<LoyaltyTransactionRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<List<LoyaltyTransaction>> GetByUserIdAsync(string userId, string tenantId, int page, int limit)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * limit;

        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   transaction_type AS TransactionType,
                   points AS Points,
                   balance_after AS BalanceAfter,
                   description AS Description,
                   reference_type AS ReferenceType,
                   reference_id AS ReferenceId,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM loyalty_transactions
            WHERE user_id = @UserId AND tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        var transactions = await connection.QueryAsync<LoyaltyTransaction>(sql, new
        {
            UserId = userId,
            TenantId = tenantId,
            Limit = limit,
            Offset = offset
        });

        return transactions.ToList();
    }

    public async Task<int> GetCountByUserIdAsync(string userId, string tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(*) 
            FROM loyalty_transactions
            WHERE user_id = @UserId AND tenant_id = @TenantId AND is_deleted = false";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, TenantId = tenantId });
    }
}
