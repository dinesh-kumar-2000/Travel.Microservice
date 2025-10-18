using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class Payment : TenantEntity<string>
{
    public string BookingId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string PaymentMethod { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? TransactionId { get; private set; }
    public string? ProviderReference { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Payment() { }

    public static Payment Create(string id, string tenantId, string bookingId, decimal amount)
    {
        return new Payment
        {
            Id = id,
            TenantId = tenantId,
            BookingId = bookingId,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete(string transactionId, string providerReference)
    {
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        ProviderReference = providerReference;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
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

