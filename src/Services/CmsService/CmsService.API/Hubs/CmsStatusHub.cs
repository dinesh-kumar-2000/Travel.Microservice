using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CmsService.API.Hubs;

[Authorize]
public class CmsStatusHub : Hub
{
    private readonly ILogger<CmsStatusHub> _logger;

    public CmsStatusHub(ILogger<CmsStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to cmsservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from cmsservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to cmsservice updates
    /// </summary>
    public async Task SubscribeToCms(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"cmsservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to cmsservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from cmsservice updates
    /// </summary>
    public async Task UnsubscribeFromCms(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cmsservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from cmsservice {UpdateType}",
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
/// Service for sending cmsservice updates to connected clients
/// </summary>
public interface ICmsNotificationService
{
    Task SendCmsUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class CmsNotificationService : ICmsNotificationService
{
    private readonly IHubContext<CmsStatusHub> _hubContext;
    private readonly ILogger<CmsNotificationService> _logger;

    public CmsNotificationService(
        IHubContext<CmsStatusHub> hubContext,
        ILogger<CmsNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendCmsUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"cmsservice-{updateType}")
            .SendAsync("cmsservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("cmsservice update sent for type {UpdateType}", updateType);
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
