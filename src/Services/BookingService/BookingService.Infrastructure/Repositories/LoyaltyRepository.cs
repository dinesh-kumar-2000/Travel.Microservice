using Dapper;
using BookingService.Domain.Entities;
using BookingService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Tenancy;

namespace BookingService.Infrastructure.Repositories;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly IDapperContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<LoyaltyRepository> _logger;

    public LoyaltyRepository(
        IDapperContext context,
        ITenantContext tenantContext,
        ILogger<LoyaltyRepository> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<LoyaltyPoints?> GetPointsByUserIdAsync(Guid userId, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
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
                   updated_at AS UpdatedAt
            FROM loyalty_points
            WHERE user_id = @UserId AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<LoyaltyPoints>(sql, 
            new { UserId = userId, TenantId = tenantId });
    }

    public async Task<LoyaltyPoints> CreatePointsAccountAsync(LoyaltyPoints points)
    {
        using var connection = _context.CreateConnection();
        
        points.Id = Guid.NewGuid();
        points.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        points.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO loyalty_points (
                id, user_id, tenant_id, current_points, lifetime_points, tier,
                points_expiring, expiry_date, created_at, updated_at
            )
            VALUES (
                @Id, @UserId, @TenantId, @CurrentPoints, @LifetimePoints, @Tier,
                @PointsExpiring, @ExpiryDate, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, points);

        _logger.LogInformation("Loyalty account created for user {UserId}", points.UserId);
        return points;
    }

    public async Task<LoyaltyPoints?> UpdatePointsAsync(LoyaltyPoints points)
    {
        using var connection = _context.CreateConnection();
        
        points.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            UPDATE loyalty_points SET
                current_points = @CurrentPoints,
                lifetime_points = @LifetimePoints,
                tier = @Tier,
                points_expiring = @PointsExpiring,
                expiry_date = @ExpiryDate,
                updated_at = @UpdatedAt
            WHERE id = @Id AND user_id = @UserId AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, points);
        return rowsAffected > 0 ? points : null;
    }

    public async Task<LoyaltyTransaction> AddTransactionAsync(LoyaltyTransaction transaction)
    {
        using var connection = _context.CreateConnection();
        
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO loyalty_transactions (
                id, user_id, tenant_id, transaction_type, points, balance_after,
                description, reference_type, reference_id, created_at
            )
            VALUES (
                @Id, @UserId, @TenantId, @TransactionType, @Points, @BalanceAfter,
                @Description, @ReferenceType, @ReferenceId, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, transaction);

        _logger.LogInformation("Loyalty transaction {Type} for {Points} points created for user {UserId}",
            transaction.TransactionType, transaction.Points, transaction.UserId);

        return transaction;
    }

    public async Task<List<LoyaltyTransaction>> GetTransactionsAsync(
        Guid userId,
        Guid tenantId,
        int page,
        int limit)
    {
        using var connection = _context.CreateConnection();
        
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
                   created_at AS CreatedAt
            FROM loyalty_transactions
            WHERE user_id = @UserId AND tenant_id = @TenantId
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

    public async Task<int> GetTransactionCountAsync(Guid userId, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT COUNT(*) 
            FROM loyalty_transactions
            WHERE user_id = @UserId AND tenant_id = @TenantId";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, TenantId = tenantId });
    }

    public async Task<List<LoyaltyReward>> GetRewardsAsync(Guid tenantId, string? category)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_available = true" };
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
                   updated_at AS UpdatedAt
            FROM loyalty_rewards
            WHERE {whereClause}
            ORDER BY points_cost ASC";

        var rewards = await connection.QueryAsync<LoyaltyReward>(sql, parameters);
        return rewards.ToList();
    }

    public async Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
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
                   updated_at AS UpdatedAt
            FROM loyalty_rewards
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<LoyaltyReward>(sql, new { Id = rewardId });
    }

    public async Task<LoyaltyReward> CreateRewardAsync(LoyaltyReward reward)
    {
        using var connection = _context.CreateConnection();
        
        reward.Id = Guid.NewGuid();
        reward.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        reward.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO loyalty_rewards (
                id, tenant_id, title, description, category, points_cost,
                value_amount, image_url, is_available, stock_quantity,
                redemption_instructions, terms_conditions, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Title, @Description, @Category, @PointsCost,
                @ValueAmount, @ImageUrl, @IsAvailable, @StockQuantity,
                @RedemptionInstructions, @TermsConditions, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, reward);
        return reward;
    }

    public async Task<LoyaltyReward?> UpdateRewardAsync(LoyaltyReward reward)
    {
        using var connection = _context.CreateConnection();
        
        reward.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE loyalty_rewards SET
                title = @Title,
                description = @Description,
                category = @Category,
                points_cost = @PointsCost,
                value_amount = @ValueAmount,
                image_url = @ImageUrl,
                is_available = @IsAvailable,
                stock_quantity = @StockQuantity,
                redemption_instructions = @RedemptionInstructions,
                terms_conditions = @TermsConditions,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, reward);
        return rowsAffected > 0 ? reward : null;
    }

    public async Task<bool> DeleteRewardAsync(Guid rewardId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM loyalty_rewards WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = rewardId });

        return rowsAffected > 0;
    }

    public async Task<LoyaltyRedemption> CreateRedemptionAsync(LoyaltyRedemption redemption)
    {
        using var connection = _context.CreateConnection();
        
        redemption.Id = Guid.NewGuid();
        redemption.RedeemedAt = DefaultProviders.DateTimeProvider.UtcNow;
        redemption.RedemptionCode = GenerateRedemptionCode();

        const string sql = @"
            INSERT INTO loyalty_redemptions (
                id, user_id, tenant_id, reward_id, points_spent,
                status, redemption_code, redeemed_at
            )
            VALUES (
                @Id, @UserId, @TenantId, @RewardId, @PointsSpent,
                @Status, @RedemptionCode, @RedeemedAt
            )";

        await connection.ExecuteAsync(sql, redemption);

        _logger.LogInformation("Reward redeemed: {RewardId} for user {UserId}",
            redemption.RewardId, redemption.UserId);

        return redemption;
    }

    public async Task<List<LoyaltyRedemption>> GetRedemptionsAsync(Guid userId, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT r.id AS Id,
                   r.user_id AS UserId,
                   r.tenant_id AS TenantId,
                   r.reward_id AS RewardId,
                   r.points_spent AS PointsSpent,
                   r.status AS Status,
                   r.redemption_code AS RedemptionCode,
                   r.redeemed_at AS RedeemedAt,
                   r.fulfilled_at AS FulfilledAt
            FROM loyalty_redemptions r
            WHERE r.user_id = @UserId AND r.tenant_id = @TenantId
            ORDER BY r.redeemed_at DESC";

        var redemptions = await connection.QueryAsync<LoyaltyRedemption>(sql, 
            new { UserId = userId, TenantId = tenantId });
        
        return redemptions.ToList();
    }

    public async Task<LoyaltyRedemption?> UpdateRedemptionAsync(LoyaltyRedemption redemption)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE loyalty_redemptions SET
                status = @Status,
                fulfilled_at = @FulfilledAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, redemption);
        return rowsAffected > 0 ? redemption : null;
    }

    public async Task<List<LoyaltyTier>> GetTiersAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   tier_name AS TierName,
                   min_points AS MinPoints,
                   discount_percentage AS DiscountPercentage,
                   benefits AS Benefits,
                   created_at AS CreatedAt
            FROM loyalty_tiers
            WHERE tenant_id = @TenantId
            ORDER BY min_points ASC";

        var tiers = await connection.QueryAsync<LoyaltyTier>(sql, new { TenantId = tenantId });
        return tiers.ToList();
    }

    public async Task<LoyaltyTier?> GetTierByNameAsync(Guid tenantId, string tierName)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   tier_name AS TierName,
                   min_points AS MinPoints,
                   discount_percentage AS DiscountPercentage,
                   benefits AS Benefits,
                   created_at AS CreatedAt
            FROM loyalty_tiers
            WHERE tenant_id = @TenantId AND tier_name = @TierName";

        return await connection.QueryFirstOrDefaultAsync<LoyaltyTier>(sql, 
            new { TenantId = tenantId, TierName = tierName });
    }

    private string GenerateRedemptionCode()
    {
        var random = new Random();
        var now = DefaultProviders.DateTimeProvider.UtcNow;
        return $"RDM{now:yyyyMMdd}{random.Next(1000, 9999)}";
    }
}

