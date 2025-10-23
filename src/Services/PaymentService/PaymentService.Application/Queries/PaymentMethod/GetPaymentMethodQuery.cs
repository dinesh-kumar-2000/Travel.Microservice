using MediatR;
using PaymentService.Contracts.Responses.PaymentMethod;

namespace PaymentService.Application.Queries.PaymentMethod;

public class GetPaymentMethodQuery : IRequest<PaymentMethodResponse?>
{
    public Guid PaymentMethodId { get; set; }
}
