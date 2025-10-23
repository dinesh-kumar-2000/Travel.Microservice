namespace PaymentService.Application.DTOs.Requests.Refund;

public class ProcessRefundRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}