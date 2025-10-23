using SharedKernel.Models;

namespace LoyaltyService.Domain.Entities;

public class LoyaltyProgram : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TierName { get; set; } = string.Empty;
    public int MinimumPoints { get; set; }
    public int? MaximumPoints { get; set; }
    public string? Benefits { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<LoyaltyMember> Members { get; set; } = new List<LoyaltyMember>();
}
