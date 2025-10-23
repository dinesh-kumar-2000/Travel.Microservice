using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CatalogService.API.Hubs;

[Authorize]
public class CatalogStatusHub : Hub
{
    private readonly ILogger<CatalogStatusHub> _logger;

    public CatalogStatusHub(ILogger<CatalogStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to catalog status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from catalog status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to catalog updates
    /// </summary>
    public async Task SubscribeToCatalog(string catalogType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"catalog-{catalogType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to catalog {CatalogType}",
            Context.ConnectionId, catalogType);
    }

    /// <summary>
    /// Unsubscribe from catalog updates
    /// </summary>
    public async Task UnsubscribeFromCatalog(string catalogType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"catalog-{catalogType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from catalog {CatalogType}",
            Context.ConnectionId, catalogType);
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
/// Service for sending catalog updates to connected clients
/// </summary>
public interface ICatalogNotificationService
{
    Task SendCatalogUpdateAsync(string catalogType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class CatalogNotificationService : ICatalogNotificationService
{
    private readonly IHubContext<CatalogStatusHub> _hubContext;
    private readonly ILogger<CatalogNotificationService> _logger;

    public CatalogNotificationService(
        IHubContext<CatalogStatusHub> hubContext,
        ILogger<CatalogNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendCatalogUpdateAsync(string catalogType, object updateData)
    {
        await _hubContext.Clients
            .Group($"catalog-{catalogType}")
            .SendAsync("catalog-update", new
            {
                catalogType = catalogType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("Catalog update sent for type {CatalogType}", catalogType);
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
