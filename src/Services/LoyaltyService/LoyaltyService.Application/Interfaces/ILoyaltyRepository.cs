using LoyaltyService.Domain.Entities;
using SharedKernel.Data;

namespace LoyaltyService.Application.Interfaces;

// Separate repositories following single responsibility principle

public interface ILoyaltyPointsRepository : ITenantBaseRepository<LoyaltyPoints, string>
{
    Task<LoyaltyPoints?> GetByUserIdAsync(string userId, string tenantId);
}

public interface ILoyaltyTransactionRepository : ITenantBaseRepository<LoyaltyTransaction, string>
{
    Task<List<LoyaltyTransaction>> GetByUserIdAsync(string userId, string tenantId, int page, int limit);
    Task<int> GetCountByUserIdAsync(string userId, string tenantId);
}

public interface ILoyaltyRewardRepository : ITenantBaseRepository<LoyaltyReward, string>
{
    Task<List<LoyaltyReward>> GetByCategoryAsync(string tenantId, string? category);
}

public interface ILoyaltyRedemptionRepository : ITenantBaseRepository<LoyaltyRedemption, string>
{
    Task<List<LoyaltyRedemption>> GetByUserIdAsync(string userId, string tenantId);
}

public interface ILoyaltyTierRepository : ITenantBaseRepository<LoyaltyTier, string>
{
    Task<LoyaltyTier?> GetByNameAsync(string tenantId, string tierName);
}
