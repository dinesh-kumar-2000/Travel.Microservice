using SharedKernel.Models;

namespace LoyaltyService.Domain.Entities;

public class Reward : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public string RewardType { get; set; } = string.Empty; // 'discount', 'freebie', 'upgrade', etc.
    public decimal? RewardValue { get; set; }
    public bool IsActive { get; set; }
}
