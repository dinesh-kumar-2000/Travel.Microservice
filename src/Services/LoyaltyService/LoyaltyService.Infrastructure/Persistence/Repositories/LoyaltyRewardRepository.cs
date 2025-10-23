using Dapper;
using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyRewardRepository : TenantBaseRepository<LoyaltyReward, string>, ILoyaltyRewardRepository
{
    protected override string TableName => "loyalty_rewards";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public LoyaltyRewardRepository(IDapperContext context, ILogger<LoyaltyRewardRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<List<LoyaltyReward>> GetByCategoryAsync(string tenantId, string? category)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_available = true", "is_deleted = false" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(category))
        {
            whereClauses.Add("category = @Category");
            parameters.Add("Category", category);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   description AS Description,
                   category AS Category,
                   points_cost AS PointsCost,
                   value_amount AS ValueAmount,
                   image_url AS ImageUrl,
                   is_available AS IsAvailable,
                   stock_quantity AS StockQuantity,
                   redemption_instructions AS RedemptionInstructions,
                   terms_conditions AS TermsConditions,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM loyalty_rewards
            WHERE {whereClause}
            ORDER BY points_cost ASC";

        var rewards = await connection.QueryAsync<LoyaltyReward>(sql, parameters);
        return rewards.ToList();
    }
}
