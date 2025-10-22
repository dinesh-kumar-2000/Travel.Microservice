using SharedKernel.Utilities;

namespace SharedKernel.SignalR;

/// <summary>
/// Interface for SignalR notification hub client methods
/// These methods are called on the client side
/// </summary>
public interface INotificationHubClient
{
    // Booking notifications
    Task ReceiveBookingCreated(BookingNotification notification);
    Task ReceiveBookingConfirmed(BookingNotification notification);
    Task ReceiveBookingCancelled(BookingNotification notification);
    
    // Payment notifications
    Task ReceivePaymentProcessing(PaymentNotification notification);
    Task ReceivePaymentCompleted(PaymentNotification notification);
    Task ReceivePaymentFailed(PaymentNotification notification);
    
    // General notifications
    Task ReceiveNotification(GeneralNotification notification);
    
    // System notifications
    Task ReceiveSystemAlert(SystemAlert alert);
    
    // User activity
    Task UserConnected(string userId);
    Task UserDisconnected(string userId);
}

public class BookingNotification : BaseNotification
{
    public string BookingId { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentNotification : BaseNotification
{
    public string PaymentId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
}

public class GeneralNotification : BaseNotification
{
    public string Id { get; set; } = DefaultProviders.IdGenerator.Generate();
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
    public Dictionary<string, object>? Data { get; set; }
}

public class SystemAlert : BaseNotification
{
    public string Severity { get; set; } = "info"; // info, warning, critical
}

