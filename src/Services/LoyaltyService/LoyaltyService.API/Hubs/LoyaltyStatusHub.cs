using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LoyaltyService.API.Hubs;

[Authorize]
public class LoyaltyStatusHub : Hub
{
    private readonly ILogger<LoyaltyStatusHub> _logger;

    public LoyaltyStatusHub(ILogger<LoyaltyStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to loyaltyservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from loyaltyservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to loyaltyservice updates
    /// </summary>
    public async Task SubscribeToLoyalty(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"loyaltyservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to loyaltyservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from loyaltyservice updates
    /// </summary>
    public async Task UnsubscribeFromLoyalty(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"loyaltyservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from loyaltyservice {UpdateType}",
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
/// Service for sending loyaltyservice updates to connected clients
/// </summary>
public interface ILoyaltyNotificationService
{
    Task SendLoyaltyUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class LoyaltyNotificationService : ILoyaltyNotificationService
{
    private readonly IHubContext<LoyaltyStatusHub> _hubContext;
    private readonly ILogger<LoyaltyNotificationService> _logger;

    public LoyaltyNotificationService(
        IHubContext<LoyaltyStatusHub> hubContext,
        ILogger<LoyaltyNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendLoyaltyUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"loyaltyservice-{updateType}")
            .SendAsync("loyaltyservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("loyaltyservice update sent for type {UpdateType}", updateType);
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
