namespace PaymentService.Contracts.Requests.PaymentMethod;

public class UpdatePaymentMethodRequest
{
    public string PaymentMethodType { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}