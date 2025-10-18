using SharedKernel.Models;

namespace BookingService.Domain.Entities;

public class Booking : TenantEntity<string>
{
    public string CustomerId { get; private set; } = string.Empty;
    public string PackageId { get; private set; } = string.Empty;
    public string BookingReference { get; private set; } = string.Empty;
    public DateTime BookingDate { get; private set; }
    public DateTime TravelDate { get; private set; }
    public int NumberOfTravelers { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public string? PaymentId { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private Booking() { }

    public static Booking Create(string id, string tenantId, string customerId, string packageId,
        DateTime travelDate, int numberOfTravelers, decimal totalAmount, string idempotencyKey)
    {
        return new Booking
        {
            Id = id,
            TenantId = tenantId,
            CustomerId = customerId,
            PackageId = packageId,
            BookingReference = GenerateReference(),
            BookingDate = DateTime.UtcNow,
            TravelDate = travelDate,
            NumberOfTravelers = numberOfTravelers,
            TotalAmount = totalAmount,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm(string paymentId)
    {
        Status = BookingStatus.Confirmed;
        PaymentId = paymentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed booking");

        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateReference()
    {
        return $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

