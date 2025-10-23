using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class Transaction : BaseEntity<string>
{
    public Guid PaymentId { get; set; }
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public int Status { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool IsActive { get; set; } = true;
}
