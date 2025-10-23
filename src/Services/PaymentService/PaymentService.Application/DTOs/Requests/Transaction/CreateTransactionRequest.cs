namespace PaymentService.Application.DTOs.Requests.Transaction;

public class CreateTransactionRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}