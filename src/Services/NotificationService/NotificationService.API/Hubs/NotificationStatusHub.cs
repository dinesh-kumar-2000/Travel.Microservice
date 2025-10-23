using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.API.Hubs;

[Authorize]
public class NotificationStatusHub : Hub
{
    private readonly ILogger<NotificationStatusHub> _logger;

    public NotificationStatusHub(ILogger<NotificationStatusHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                  ?? Context.User?.FindFirst("userId")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation("User {UserId} connected to notificationservice status hub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                  ?? Context.User?.FindFirst("userId")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation("User {UserId} disconnected from notificationservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to notificationservice updates
    /// </summary>
    public async Task SubscribeToNotification(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"notificationservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to notificationservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from notificationservice updates
    /// </summary>
    public async Task UnsubscribeFromNotification(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notificationservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from notificationservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Send heartbeat/ping
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("pong");
    }
}

/// <summary>
/// Service for sending notificationservice updates to connected clients
/// </summary>
public interface INotificationNotificationService
{
    Task SendNotificationUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class NotificationNotificationService : INotificationNotificationService
{
    private readonly IHubContext<NotificationStatusHub> _hubContext;
    private readonly ILogger<NotificationNotificationService> _logger;

    public NotificationNotificationService(
        IHubContext<NotificationStatusHub> hubContext,
        ILogger<NotificationNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"notificationservice-{updateType}")
            .SendAsync("notificationservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("notificationservice update sent for type {UpdateType}", updateType);
    }

    public async Task SendUserNotificationAsync(Guid userId, object notification)
    {
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("notification", new
            {
                userId = userId,
                data = notification,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("Notification sent to user {UserId}", userId);
    }
}
