using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TenantService.API.Hubs;

[Authorize]
public class TenantStatusHub : Hub
{
    private readonly ILogger<TenantStatusHub> _logger;

    public TenantStatusHub(ILogger<TenantStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to tenantservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from tenantservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to tenantservice updates
    /// </summary>
    public async Task SubscribeToTenant(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tenantservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to tenantservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from tenantservice updates
    /// </summary>
    public async Task UnsubscribeFromTenant(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenantservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from tenantservice {UpdateType}",
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
/// Service for sending tenantservice updates to connected clients
/// </summary>
public interface ITenantNotificationService
{
    Task SendTenantUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class TenantNotificationService : ITenantNotificationService
{
    private readonly IHubContext<TenantStatusHub> _hubContext;
    private readonly ILogger<TenantNotificationService> _logger;

    public TenantNotificationService(
        IHubContext<TenantStatusHub> hubContext,
        ILogger<TenantNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendTenantUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"tenantservice-{updateType}")
            .SendAsync("tenantservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("tenantservice update sent for type {UpdateType}", updateType);
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
