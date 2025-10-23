using LoyaltyService.Domain.Entities;
using LoyaltyService.Domain.Repositories;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Repositories;

public class RewardRepository : BaseRepository<Reward>, IRewardRepository
{
    public RewardRepository(DapperContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reward>> GetActiveRewardsAsync()
    {
        const string sql = @"
            SELECT * FROM rewards 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY points_cost";
        
        return await QueryAsync<Reward>(sql);
    }

    public async Task<IEnumerable<Reward>> GetRewardsByTypeAsync(string rewardType)
    {
        const string sql = @"
            SELECT * FROM rewards 
            WHERE reward_type = @RewardType AND is_active = true AND is_deleted = false 
            ORDER BY points_cost";
        
        return await QueryAsync<Reward>(sql, new { RewardType = rewardType });
    }
}
