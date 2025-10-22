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

    /// <summary>
    /// Helper method to eliminate duplicate try-catch logging patterns
    /// </summary>
    private async Task ExecuteWithLoggingAsync(
        Func<Task> action,
        string operationName,
        LogLevel successLevel = LogLevel.Debug,
        params object[] logParameters)
    {
        try
        {
            await action();
            _logger.Log(successLevel, $"{operationName} completed successfully", logParameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during {operationName}", logParameters);
        }
    }

    public async Task SendToUserAsync(string userId, string method, object data)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                }),
            "Send notification to user",
            LogLevel.Debug,
            userId, method);
    }

    public async Task SendToTenantAsync(string tenantId, string method, object data)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForTenant(tenantId))
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                }),
            "Send notification to tenant",
            LogLevel.Debug,
            tenantId, method);
    }

    public async Task SendToAllAsync(string method, object data)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients.All
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                }),
            "Send notification to all clients",
            LogLevel.Debug,
            method);
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(groupName)
                .ReceiveNotification(new GeneralNotification
                {
                    Type = method,
                    Data = data as Dictionary<string, object>
                }),
            "Send notification to group",
            LogLevel.Debug,
            groupName, method);
    }

    public async Task NotifyBookingCreatedAsync(string userId, BookingNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceiveBookingCreated(notification),
            "Notify booking created",
            LogLevel.Information,
            userId, notification.BookingId);
    }

    public async Task NotifyBookingConfirmedAsync(string userId, BookingNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceiveBookingConfirmed(notification),
            "Notify booking confirmed",
            LogLevel.Information,
            userId, notification.BookingId);
    }

    public async Task NotifyBookingCancelledAsync(string userId, BookingNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceiveBookingCancelled(notification),
            "Notify booking cancelled",
            LogLevel.Information,
            userId, notification.BookingId);
    }

    public async Task NotifyPaymentCompletedAsync(string userId, PaymentNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceivePaymentCompleted(notification),
            "Notify payment completed",
            LogLevel.Information,
            userId, notification.PaymentId);
    }

    public async Task NotifyPaymentFailedAsync(string userId, PaymentNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceivePaymentFailed(notification),
            "Notify payment failed",
            LogLevel.Information,
            userId, notification.PaymentId);
    }

    public async Task SendNotificationAsync(string userId, GeneralNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForUser(userId))
                .ReceiveNotification(notification),
            "Send general notification",
            LogLevel.Debug,
            userId, notification.Title);
    }

    public async Task BroadcastToTenantAsync(string tenantId, GeneralNotification notification)
    {
        await ExecuteWithLoggingAsync(
            () => _hubContext.Clients
                .Group(SignalRGroupNames.ForTenant(tenantId))
                .ReceiveNotification(notification),
            "Broadcast notification to tenant",
            LogLevel.Debug,
            tenantId, notification.Title);
    }
}

