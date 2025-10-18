namespace SharedKernel.SignalR;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public interface ISignalRNotificationService
{
    // Send to specific user
    Task SendToUserAsync(string userId, string method, object data);
    
    // Send to all users in tenant
    Task SendToTenantAsync(string tenantId, string method, object data);
    
    // Send to all users
    Task SendToAllAsync(string method, object data);
    
    // Send to specific group
    Task SendToGroupAsync(string groupName, string method, object data);
    
    // Booking notifications
    Task NotifyBookingCreatedAsync(string userId, BookingNotification notification);
    Task NotifyBookingConfirmedAsync(string userId, BookingNotification notification);
    Task NotifyBookingCancelledAsync(string userId, BookingNotification notification);
    
    // Payment notifications
    Task NotifyPaymentCompletedAsync(string userId, PaymentNotification notification);
    Task NotifyPaymentFailedAsync(string userId, PaymentNotification notification);
    
    // General notifications
    Task SendNotificationAsync(string userId, GeneralNotification notification);
    Task BroadcastToTenantAsync(string tenantId, GeneralNotification notification);
}

