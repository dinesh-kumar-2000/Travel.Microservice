using MediatR;
using PaymentService.Application.DTOs.Responses.PaymentMethod;

namespace PaymentService.Application.Queries.PaymentMethod;

public class GetPaymentMethodQuery : IRequest<PaymentMethodResponse?>
{
    public Guid PaymentMethodId { get; set; }
}
