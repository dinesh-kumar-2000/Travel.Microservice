using Dapper;
using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyPointsRepository : TenantBaseRepository<LoyaltyPoints, string>, ILoyaltyPointsRepository
{
    protected override string TableName => "loyalty_points";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public LoyaltyPointsRepository(IDapperContext context, ILogger<LoyaltyPointsRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<LoyaltyPoints?> GetByUserIdAsync(string userId, string tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   current_points AS CurrentPoints,
                   lifetime_points AS LifetimePoints,
                   tier AS Tier,
                   points_expiring AS PointsExpiring,
                   expiry_date AS ExpiryDate,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM loyalty_points
            WHERE user_id = @UserId AND tenant_id = @TenantId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<LoyaltyPoints>(sql, 
            new { UserId = userId, TenantId = tenantId });
    }
}
