using SharedKernel.Models;

namespace LoyaltyService.Domain.Entities;

public class LoyaltyMember : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProgramId { get; set; }
    public int PointsBalance { get; set; }
    public int TierLevel { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int TotalPointsEarned { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual LoyaltyProgram Program { get; set; } = null!;
    public virtual ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();
}
