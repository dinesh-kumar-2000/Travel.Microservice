using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IdentityService.API.Hubs;

[Authorize]
public class IdentityStatusHub : Hub
{
    private readonly ILogger<IdentityStatusHub> _logger;

    public IdentityStatusHub(ILogger<IdentityStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to identityservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from identityservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to identityservice updates
    /// </summary>
    public async Task SubscribeToIdentity(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"identityservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to identityservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from identityservice updates
    /// </summary>
    public async Task UnsubscribeFromIdentity(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"identityservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from identityservice {UpdateType}",
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
/// Service for sending identityservice updates to connected clients
/// </summary>
public interface IIdentityNotificationService
{
    Task SendIdentityUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class IdentityNotificationService : IIdentityNotificationService
{
    private readonly IHubContext<IdentityStatusHub> _hubContext;
    private readonly ILogger<IdentityNotificationService> _logger;

    public IdentityNotificationService(
        IHubContext<IdentityStatusHub> hubContext,
        ILogger<IdentityNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendIdentityUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"identityservice-{updateType}")
            .SendAsync("identityservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("identityservice update sent for type {UpdateType}", updateType);
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
