namespace PaymentService.Application.DTOs.Requests.PaymentMethod;

public class AddPaymentMethodRequest
{
    public string UserId { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = string.Empty; // CreditCard, DebitCard, PayPal, etc.
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}