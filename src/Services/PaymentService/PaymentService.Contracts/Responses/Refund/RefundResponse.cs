namespace PaymentService.Contracts.Responses.Refund;

public class RefundResponse
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int Status { get; set; }
    public DateTime RefundDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
