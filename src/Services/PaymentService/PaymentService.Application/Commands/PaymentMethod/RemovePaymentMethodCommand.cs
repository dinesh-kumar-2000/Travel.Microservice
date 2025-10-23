using MediatR;

namespace PaymentService.Application.Commands.PaymentMethod;

public class RemovePaymentMethodCommand : IRequest<Unit>
{
    public Guid PaymentMethodId { get; set; }
}
