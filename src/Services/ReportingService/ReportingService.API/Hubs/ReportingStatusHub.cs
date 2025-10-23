using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ReportingService.API.Hubs;

[Authorize]
public class ReportingStatusHub : Hub
{
    private readonly ILogger<ReportingStatusHub> _logger;

    public ReportingStatusHub(ILogger<ReportingStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to reportingservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from reportingservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to reportingservice updates
    /// </summary>
    public async Task SubscribeToReporting(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"reportingservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to reportingservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from reportingservice updates
    /// </summary>
    public async Task UnsubscribeFromReporting(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"reportingservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from reportingservice {UpdateType}",
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
/// Service for sending reportingservice updates to connected clients
/// </summary>
public interface IReportingNotificationService
{
    Task SendReportingUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class ReportingNotificationService : IReportingNotificationService
{
    private readonly IHubContext<ReportingStatusHub> _hubContext;
    private readonly ILogger<ReportingNotificationService> _logger;

    public ReportingNotificationService(
        IHubContext<ReportingStatusHub> hubContext,
        ILogger<ReportingNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendReportingUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"reportingservice-{updateType}")
            .SendAsync("reportingservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("reportingservice update sent for type {UpdateType}", updateType);
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
