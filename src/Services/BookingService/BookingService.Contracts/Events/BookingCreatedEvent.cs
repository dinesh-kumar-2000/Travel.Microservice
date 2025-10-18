using EventBus.Events;

namespace BookingService.Contracts.Events;

public class BookingCreatedEvent : IntegrationEvent
{
    public string BookingId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BookingConfirmedEvent : IntegrationEvent
{
    public string BookingId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
}

public class BookingCancelledEvent : IntegrationEvent
{
    public string BookingId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

