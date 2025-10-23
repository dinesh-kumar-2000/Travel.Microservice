using Dapper;
using LoyaltyService.Domain.Entities;
using LoyaltyService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Repositories;

public class LoyaltyTierRepository : TenantBaseRepository<LoyaltyTier, string>, ILoyaltyTierRepository
{
    protected override string TableName => "loyalty_tiers";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public LoyaltyTierRepository(IDapperContext context, ILogger<LoyaltyTierRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<LoyaltyTier?> GetByNameAsync(string tenantId, string tierName)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   tier_name AS TierName,
                   min_points AS MinPoints,
                   discount_percentage AS DiscountPercentage,
                   benefits AS Benefits,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM loyalty_tiers
            WHERE tenant_id = @TenantId AND tier_name = @TierName AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<LoyaltyTier>(sql, 
            new { TenantId = tenantId, TierName = tierName });
    }
}
