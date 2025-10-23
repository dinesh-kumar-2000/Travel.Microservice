namespace PaymentService.Domain.Enums;

public enum PaymentGatewayType
{
    Stripe = 1,
    PayPal = 2,
    Razorpay = 3,
    Square = 4,
    Adyen = 5,
    Braintree = 6,
    AuthorizeNet = 7,
    Worldpay = 8,
    Custom = 9
}
