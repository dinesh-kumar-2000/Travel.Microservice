using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ReviewsService.API.Hubs;

[Authorize]
public class ReviewsStatusHub : Hub
{
    private readonly ILogger<ReviewsStatusHub> _logger;

    public ReviewsStatusHub(ILogger<ReviewsStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to reviewsservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from reviewsservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to reviewsservice updates
    /// </summary>
    public async Task SubscribeToReviews(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"reviewsservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to reviewsservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from reviewsservice updates
    /// </summary>
    public async Task UnsubscribeFromReviews(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"reviewsservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from reviewsservice {UpdateType}",
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
/// Service for sending reviewsservice updates to connected clients
/// </summary>
public interface IReviewsNotificationService
{
    Task SendReviewsUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class ReviewsNotificationService : IReviewsNotificationService
{
    private readonly IHubContext<ReviewsStatusHub> _hubContext;
    private readonly ILogger<ReviewsNotificationService> _logger;

    public ReviewsNotificationService(
        IHubContext<ReviewsStatusHub> hubContext,
        ILogger<ReviewsNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendReviewsUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"reviewsservice-{updateType}")
            .SendAsync("reviewsservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("reviewsservice update sent for type {UpdateType}", updateType);
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
