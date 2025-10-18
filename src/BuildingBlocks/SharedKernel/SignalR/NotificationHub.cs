using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SharedKernel.SignalR;

[Authorize]
public class NotificationHub : Hub<INotificationHubClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(tenantId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(connectionId, $"user:{userId}");
            
            // Add user to tenant group
            await Groups.AddToGroupAsync(connectionId, $"tenant:{tenantId}");
            
            _logger.LogInformation(
                "User {UserId} from tenant {TenantId} connected with connection {ConnectionId}",
                userId, tenantId, connectionId);
            
            // Notify others in tenant
            await Clients.Group($"tenant:{tenantId}")
                .UserConnected(userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(tenantId))
        {
            _logger.LogInformation(
                "User {UserId} from tenant {TenantId} disconnected",
                userId, tenantId);
            
            // Notify others in tenant
            await Clients.Group($"tenant:{tenantId}")
                .UserDisconnected(userId);
        }

        if (exception != null)
        {
            _logger.LogError(exception, "User disconnected with error");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific notification types
    /// </summary>
    public async Task SubscribeToNotifications(string[] notificationTypes)
    {
        foreach (var type in notificationTypes)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications:{type}");
        }
        
        _logger.LogInformation("User subscribed to notification types: {Types}", 
            string.Join(", ", notificationTypes));
    }

    /// <summary>
    /// Unsubscribe from notification types
    /// </summary>
    public async Task UnsubscribeFromNotifications(string[] notificationTypes)
    {
        foreach (var type in notificationTypes)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications:{type}");
        }
        
        _logger.LogInformation("User unsubscribed from notification types: {Types}", 
            string.Join(", ", notificationTypes));
    }

    /// <summary>
    /// Send typing indicator (for chat features)
    /// </summary>
    public async Task SendTypingIndicator(string recipientUserId)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(senderId))
        {
            await Clients.Group($"user:{recipientUserId}")
                .ReceiveNotification(new GeneralNotification
                {
                    Type = "typing",
                    Message = $"User {senderId} is typing...",
                    Data = new Dictionary<string, object> { ["senderId"] = senderId }
                });
        }
    }
}

