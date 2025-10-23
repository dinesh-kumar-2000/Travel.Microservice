using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SupportService.API.Hubs;

[Authorize]
public class SupportStatusHub : Hub
{
    private readonly ILogger<SupportStatusHub> _logger;

    public SupportStatusHub(ILogger<SupportStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to supportservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from supportservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to supportservice updates
    /// </summary>
    public async Task SubscribeToSupport(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"supportservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to supportservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from supportservice updates
    /// </summary>
    public async Task UnsubscribeFromSupport(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"supportservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from supportservice {UpdateType}",
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
/// Service for sending supportservice updates to connected clients
/// </summary>
public interface ISupportNotificationService
{
    Task SendSupportUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class SupportNotificationService : ISupportNotificationService
{
    private readonly IHubContext<SupportStatusHub> _hubContext;
    private readonly ILogger<SupportNotificationService> _logger;

    public SupportNotificationService(
        IHubContext<SupportStatusHub> hubContext,
        ILogger<SupportNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendSupportUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"supportservice-{updateType}")
            .SendAsync("supportservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("supportservice update sent for type {UpdateType}", updateType);
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
