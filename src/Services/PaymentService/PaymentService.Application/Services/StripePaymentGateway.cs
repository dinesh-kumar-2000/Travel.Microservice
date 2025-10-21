using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace PaymentService.Application.Services;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public string ProviderName => "stripe";

    public StripePaymentGateway(
        IConfiguration configuration,
        ILogger<StripePaymentGateway> logger)
    {
        _logger = logger;
        _secretKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe:SecretKey not configured");
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string customerEmail,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe payment intent for {Amount} {Currency}", amount, currency);

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                ReceiptEmail = customerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "customer_email", customerEmail }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe payment intent created: {PaymentIntentId}", paymentIntent.Id);

            return new PaymentIntentResult(
                true,
                paymentIntent.Id,
                paymentIntent.ClientSecret,
                null,
                null
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment intent creation failed");
            return new PaymentIntentResult(false, null, null, null, ex.Message);
        }
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            
            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to confirm Stripe payment {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        string paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe refund for payment {PaymentId}, amount {Amount}", paymentId, amount);

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentId,
                Amount = (long)(amount * 100),
                Reason = "requested_by_customer"
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe refund created: {RefundId}", refund.Id);

            return new RefundResult(true, refund.Id, amount, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed for payment {PaymentId}", paymentId);
            return new RefundResult(false, null, 0, ex.Message);
        }
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
            return Task.FromResult(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return Task.FromResult(false);
        }
    }
}

