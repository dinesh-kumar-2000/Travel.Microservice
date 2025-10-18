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

public class BookingNotification
{
    public string BookingId { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
}

public class PaymentNotification
{
    public string PaymentId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
}

public class GeneralNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Data { get; set; }
}

public class SystemAlert
{
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // info, warning, critical
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

