using MediatR;
using PaymentService.Contracts.Responses.Refund;

namespace PaymentService.Application.Queries.Refund;

public class GetRefundQuery : IRequest<RefundResponse?>
{
    public Guid RefundId { get; set; }
}
