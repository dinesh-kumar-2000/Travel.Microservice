using MediatR;
using PaymentService.Application.DTOs.Responses.Refund;

namespace PaymentService.Application.Queries.Refund;

public class GetRefundQuery : IRequest<RefundResponse?>
{
    public Guid RefundId { get; set; }
}
