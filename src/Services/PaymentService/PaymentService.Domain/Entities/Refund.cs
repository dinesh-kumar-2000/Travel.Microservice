using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class Refund : BaseEntity<string>
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reason { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int Status { get; set; }
    public DateTime RefundDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
