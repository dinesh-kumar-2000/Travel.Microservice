using MediatR;
using PaymentService.Application.DTOs.Responses.Transaction;

namespace PaymentService.Application.Commands.Transaction;

public class UpdateTransactionCommand : IRequest<TransactionResponse>
{
    public Guid TransactionId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
