namespace PaymentService.Application.Services;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string customerEmail,
        string? returnUrl = null,
        CancellationToken cancellationToken = default);
    Task<bool> ConfirmPaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);
    Task<RefundResult> RefundPaymentAsync(
        string paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signature);
}

public record PaymentIntentResult(
    bool Success,
    string? PaymentIntentId,
    string? ClientSecret,
    string? CheckoutUrl,
    string? ErrorMessage
);

public record RefundResult(
    bool Success,
    string? RefundId,
    decimal RefundedAmount,
    string? ErrorMessage
);

