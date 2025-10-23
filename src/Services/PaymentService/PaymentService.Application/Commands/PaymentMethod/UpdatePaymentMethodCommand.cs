using MediatR;
using PaymentService.Contracts.Responses.PaymentMethod;

namespace PaymentService.Application.Commands.PaymentMethod;

public class UpdatePaymentMethodCommand : IRequest<PaymentMethodResponse>
{
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodType { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
