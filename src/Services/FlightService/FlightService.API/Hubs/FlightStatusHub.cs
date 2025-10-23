using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlightService.API.Hubs;

[Authorize]
public class FlightStatusHub : Hub
{
    private readonly ILogger<FlightStatusHub> _logger;

    public FlightStatusHub(ILogger<FlightStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to flightservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from flightservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to flightservice updates
    /// </summary>
    public async Task SubscribeToFlight(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"flightservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to flightservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from flightservice updates
    /// </summary>
    public async Task UnsubscribeFromFlight(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"flightservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from flightservice {UpdateType}",
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
/// Service for sending flightservice updates to connected clients
/// </summary>
public interface IFlightNotificationService
{
    Task SendFlightUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class FlightNotificationService : IFlightNotificationService
{
    private readonly IHubContext<FlightStatusHub> _hubContext;
    private readonly ILogger<FlightNotificationService> _logger;

    public FlightNotificationService(
        IHubContext<FlightStatusHub> hubContext,
        ILogger<FlightNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendFlightUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"flightservice-{updateType}")
            .SendAsync("flightservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("flightservice update sent for type {UpdateType}", updateType);
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
