using MediatR;
using PaymentService.Contracts.Responses.Refund;

namespace PaymentService.Application.Commands.Refund;

public class ProcessRefundCommand : IRequest<RefundResponse>
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reason { get; set; } = string.Empty;
    public string? Reference { get; set; }
}
