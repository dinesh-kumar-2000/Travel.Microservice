using BookingService.Domain.Entities;

namespace BookingService.Domain.Repositories;

public interface ILoyaltyRepository
{
    // Loyalty Points
    Task<LoyaltyPoints?> GetPointsByUserIdAsync(Guid userId, Guid tenantId);
    Task<LoyaltyPoints> CreatePointsAccountAsync(LoyaltyPoints points);
    Task<LoyaltyPoints?> UpdatePointsAsync(LoyaltyPoints points);
    
    // Transactions
    Task<LoyaltyTransaction> AddTransactionAsync(LoyaltyTransaction transaction);
    Task<List<LoyaltyTransaction>> GetTransactionsAsync(Guid userId, Guid tenantId, int page, int limit);
    Task<int> GetTransactionCountAsync(Guid userId, Guid tenantId);
    
    // Rewards
    Task<List<LoyaltyReward>> GetRewardsAsync(Guid tenantId, string? category);
    Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId);
    Task<LoyaltyReward> CreateRewardAsync(LoyaltyReward reward);
    Task<LoyaltyReward?> UpdateRewardAsync(LoyaltyReward reward);
    Task<bool> DeleteRewardAsync(Guid rewardId);
    
    // Redemptions
    Task<LoyaltyRedemption> CreateRedemptionAsync(LoyaltyRedemption redemption);
    Task<List<LoyaltyRedemption>> GetRedemptionsAsync(Guid userId, Guid tenantId);
    Task<LoyaltyRedemption?> UpdateRedemptionAsync(LoyaltyRedemption redemption);
    
    // Tiers
    Task<List<LoyaltyTier>> GetTiersAsync(Guid tenantId);
    Task<LoyaltyTier?> GetTierByNameAsync(Guid tenantId, string tierName);
}

