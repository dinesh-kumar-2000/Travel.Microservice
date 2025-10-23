using Dapper;
using LoyaltyService.Domain.Entities;
using LoyaltyService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Repositories;

public class LoyaltyRedemptionRepository : TenantBaseRepository<LoyaltyRedemption, string>, ILoyaltyRedemptionRepository
{
    protected override string TableName => "loyalty_redemptions";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public LoyaltyRedemptionRepository(IDapperContext context, ILogger<LoyaltyRedemptionRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<List<LoyaltyRedemption>> GetByUserIdAsync(string userId, string tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   reward_id AS RewardId,
                   points_spent AS PointsSpent,
                   status AS Status,
                   redemption_code AS RedemptionCode,
                   redeemed_at AS RedeemedAt,
                   fulfilled_at AS FulfilledAt,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM loyalty_redemptions
            WHERE user_id = @UserId AND tenant_id = @TenantId AND is_deleted = false
            ORDER BY redeemed_at DESC";

        var redemptions = await connection.QueryAsync<LoyaltyRedemption>(sql, 
            new { UserId = userId, TenantId = tenantId });
        
        return redemptions.ToList();
    }
}
