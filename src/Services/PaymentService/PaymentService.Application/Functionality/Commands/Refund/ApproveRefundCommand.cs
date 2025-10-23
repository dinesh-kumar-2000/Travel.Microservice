using MediatR;
using PaymentService.Application.DTOs.Responses.Refund;

namespace PaymentService.Application.Commands.Refund;

public class ApproveRefundCommand : IRequest<RefundResponse>
{
    public Guid RefundId { get; set; }
}
