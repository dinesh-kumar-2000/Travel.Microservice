using LoyaltyService.Domain.Entities;
using SharedKernel.Models;

namespace LoyaltyService.Domain.Repositories;

public interface IRewardRepository : IRepository<Reward>
{
    Task<IEnumerable<Reward>> GetActiveRewardsAsync();
    Task<IEnumerable<Reward>> GetRewardsByTypeAsync(string rewardType);
}
