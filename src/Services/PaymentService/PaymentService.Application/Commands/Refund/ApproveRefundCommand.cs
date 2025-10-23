using MediatR;
using PaymentService.Contracts.Responses.Refund;

namespace PaymentService.Application.Commands.Refund;

public class ApproveRefundCommand : IRequest<RefundResponse>
{
    public Guid RefundId { get; set; }
}
