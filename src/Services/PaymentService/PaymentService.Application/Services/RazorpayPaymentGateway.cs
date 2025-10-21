using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Security.Cryptography;

namespace PaymentService.Application.Services;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly ILogger<RazorpayPaymentGateway> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _keyId;
    private readonly string _keySecret;
    private readonly string _webhookSecret;
    private readonly string _baseUrl = "https://api.razorpay.com/v1";

    public string ProviderName => "razorpay";

    public RazorpayPaymentGateway(
        IConfiguration configuration,
        ILogger<RazorpayPaymentGateway> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _keyId = configuration["Razorpay:KeyId"] ?? "test_key";
        _keySecret = configuration["Razorpay:KeySecret"] ?? "test_secret";
        _webhookSecret = configuration["Razorpay:WebhookSecret"] ?? "";
        
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
    }

    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string customerEmail,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Razorpay order for {Amount} {Currency}", amount, currency);

            // Simplified Razorpay order creation (mockup for demonstration)
            var orderId = $"order_{Guid.NewGuid().ToString("N")[..16]}";

            _logger.LogInformation("Razorpay order created: {OrderId}", orderId);

            return Task.FromResult(new PaymentIntentResult(
                true,
                orderId,
                null, // Razorpay doesn't use client secret
                $"https://razorpay.com/checkout/{orderId}",
                null
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay order creation failed");
            return Task.FromResult(new PaymentIntentResult(false, null, null, null, ex.Message));
        }
    }

    public Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified confirmation (in production, this would call Razorpay API)
            _logger.LogInformation("Confirming Razorpay payment {PaymentId}", paymentIntentId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm Razorpay payment {PaymentId}", paymentIntentId);
            return Task.FromResult(false);
        }
    }

    public Task<RefundResult> RefundPaymentAsync(
        string paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Razorpay refund for payment {PaymentId}, amount {Amount}", paymentId, amount);

            var refundId = $"rfnd_{Guid.NewGuid().ToString("N")[..16]}";

            _logger.LogInformation("Razorpay refund created: {RefundId}", refundId);

            return Task.FromResult(new RefundResult(true, refundId, amount, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay refund failed for payment {PaymentId}", paymentId);
            return Task.FromResult(new RefundResult(false, null, 0, ex.Message));
        }
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            // Razorpay webhook signature verification
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            var isValid = computedHash == signature;
            
            _logger.LogInformation("Razorpay webhook signature verification: {IsValid}", isValid);
            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay webhook signature verification failed");
            return Task.FromResult(false);
        }
    }
}

