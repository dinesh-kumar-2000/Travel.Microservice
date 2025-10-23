using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SeoInsuranceService.API.Hubs;

[Authorize]
public class SeoInsuranceStatusHub : Hub
{
    private readonly ILogger<SeoInsuranceStatusHub> _logger;

    public SeoInsuranceStatusHub(ILogger<SeoInsuranceStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to seoinsuranceservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from seoinsuranceservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to seoinsuranceservice updates
    /// </summary>
    public async Task SubscribeToSeoInsurance(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"seoinsuranceservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to seoinsuranceservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from seoinsuranceservice updates
    /// </summary>
    public async Task UnsubscribeFromSeoInsurance(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"seoinsuranceservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from seoinsuranceservice {UpdateType}",
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
/// Service for sending seoinsuranceservice updates to connected clients
/// </summary>
public interface ISeoInsuranceNotificationService
{
    Task SendSeoInsuranceUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class SeoInsuranceNotificationService : ISeoInsuranceNotificationService
{
    private readonly IHubContext<SeoInsuranceStatusHub> _hubContext;
    private readonly ILogger<SeoInsuranceNotificationService> _logger;

    public SeoInsuranceNotificationService(
        IHubContext<SeoInsuranceStatusHub> hubContext,
        ILogger<SeoInsuranceNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendSeoInsuranceUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"seoinsuranceservice-{updateType}")
            .SendAsync("seoinsuranceservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("seoinsuranceservice update sent for type {UpdateType}", updateType);
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
