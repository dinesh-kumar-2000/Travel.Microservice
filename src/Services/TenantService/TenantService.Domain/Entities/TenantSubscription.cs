using SharedKernel.Models;
using TenantService.Domain.Enums;

namespace TenantService.Domain.Entities;

public class TenantSubscription : BaseEntity<string>
{
    public Guid TenantId { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal MonthlyPrice { get; set; }
    public string? PaymentMethodId { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
}
