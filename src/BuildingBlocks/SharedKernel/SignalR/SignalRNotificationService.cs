using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SharedKernel.SignalR;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub, INotificationHubClient> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(string userId, string method, object data)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                });
            
            _logger.LogDebug("Sent SignalR notification to user {UserId}, method: {Method}", userId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR notification to user {UserId}", userId);
        }
    }

    public async Task SendToTenantAsync(string tenantId, string method, object data)
    {
        try
        {
            await _hubContext.Clients
                .Group($"tenant:{tenantId}")
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                });
            
            _logger.LogDebug("Sent SignalR notification to tenant {TenantId}, method: {Method}", tenantId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR notification to tenant {TenantId}", tenantId);
        }
    }

    public async Task SendToAllAsync(string method, object data)
    {
        try
        {
            await _hubContext.Clients.All
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                });
            
            _logger.LogDebug("Sent SignalR notification to all clients, method: {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR notification to all clients");
        }
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                });
            
            _logger.LogDebug("Sent SignalR notification to group {GroupName}, method: {Method}", groupName, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyBookingCreatedAsync(string userId, BookingNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveBookingCreated(notification);
            
            _logger.LogInformation("Sent booking created notification to user {UserId} for booking {BookingId}", 
                userId, notification.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking created notification");
        }
    }

    public async Task NotifyBookingConfirmedAsync(string userId, BookingNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveBookingConfirmed(notification);
            
            _logger.LogInformation("Sent booking confirmed notification to user {UserId} for booking {BookingId}", 
                userId, notification.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking confirmed notification");
        }
    }

    public async Task NotifyBookingCancelledAsync(string userId, BookingNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveBookingCancelled(notification);
            
            _logger.LogInformation("Sent booking cancelled notification to user {UserId} for booking {BookingId}", 
                userId, notification.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking cancelled notification");
        }
    }

    public async Task NotifyPaymentCompletedAsync(string userId, PaymentNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceivePaymentCompleted(notification);
            
            _logger.LogInformation("Sent payment completed notification to user {UserId} for payment {PaymentId}", 
                userId, notification.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment completed notification");
        }
    }

    public async Task NotifyPaymentFailedAsync(string userId, PaymentNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceivePaymentFailed(notification);
            
            _logger.LogInformation("Sent payment failed notification to user {UserId} for payment {PaymentId}", 
                userId, notification.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment failed notification");
        }
    }

    public async Task SendNotificationAsync(string userId, GeneralNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveNotification(notification);
            
            _logger.LogDebug("Sent general notification to user {UserId}: {Title}", 
                userId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending general notification to user {UserId}", userId);
        }
    }

    public async Task BroadcastToTenantAsync(string tenantId, GeneralNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"tenant:{tenantId}")
                .ReceiveNotification(notification);
            
            _logger.LogDebug("Broadcasted notification to tenant {TenantId}: {Title}", 
                tenantId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification to tenant {TenantId}", tenantId);
        }
    }
}

