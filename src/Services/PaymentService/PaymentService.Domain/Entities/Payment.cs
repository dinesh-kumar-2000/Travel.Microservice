using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class Payment : TenantEntity<string>
{
    public string BookingId { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string PaymentMethod { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? TransactionId { get; private set; }
    public string? ProviderReference { get; private set; }
    public string? ProviderName { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public decimal? RefundAmount { get; private set; }
    public string? RefundReason { get; private set; }

    private Payment() { }

    public static Payment Create(string id, string tenantId, string bookingId, string customerId,
        string customerEmail, decimal amount, string currency, string paymentMethod, string providerName)
    {
        return new Payment
        {
            Id = id,
            TenantId = tenantId,
            BookingId = bookingId,
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            Amount = amount,
            Currency = currency,
            PaymentMethod = paymentMethod,
            ProviderName = providerName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetPaymentIntent(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProcessing()
    {
        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string transactionId, string providerReference)
    {
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        ProviderReference = providerReference;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string? errorCode = null, string? errorMessage = null)
    {
        Status = PaymentStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        FailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund(decimal refundAmount, string reason)
    {
        Status = PaymentStatus.Refunded;
        RefundAmount = refundAmount;
        RefundReason = reason;
        RefundedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

