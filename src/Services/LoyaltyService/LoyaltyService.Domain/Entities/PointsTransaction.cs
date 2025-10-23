using SharedKernel.Models;

namespace LoyaltyService.Domain.Entities;

public class PointsTransaction : BaseEntity
{
    public Guid MemberId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // 'earned', 'redeemed', 'expired', 'adjusted'
    public int PointsAmount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; } // 'booking', 'review', 'referral', etc.
    public Guid? ReferenceId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Navigation properties
    public virtual LoyaltyMember Member { get; set; } = null!;
}
