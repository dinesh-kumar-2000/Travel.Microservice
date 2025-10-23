using LoyaltyService.Domain.Entities;
using SharedKernel.Models;

namespace LoyaltyService.Application.Interfaces;

public interface IRewardRepository : IRepository<Reward>
{
    Task<IEnumerable<Reward>> GetActiveRewardsAsync();
    Task<IEnumerable<Reward>> GetRewardsByTypeAsync(string rewardType);
}
