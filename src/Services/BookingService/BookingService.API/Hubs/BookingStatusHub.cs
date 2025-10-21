using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BookingService.API.Hubs;

[Authorize]
public class BookingStatusHub : Hub
{
    private readonly ILogger<BookingStatusHub> _logger;

    public BookingStatusHub(ILogger<BookingStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to booking status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from booking status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to booking updates
    /// </summary>
    public async Task SubscribeToBooking(string bookingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to booking {BookingId}",
            Context.ConnectionId, bookingId);
    }

    /// <summary>
    /// Unsubscribe from booking updates
    /// </summary>
    public async Task UnsubscribeFromBooking(string bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from booking {BookingId}",
            Context.ConnectionId, bookingId);
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
/// Service for sending booking updates to connected clients
/// </summary>
public interface IBookingNotificationService
{
    Task SendBookingUpdateAsync(Guid bookingId, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class BookingNotificationService : IBookingNotificationService
{
    private readonly IHubContext<BookingStatusHub> _hubContext;
    private readonly ILogger<BookingNotificationService> _logger;

    public BookingNotificationService(
        IHubContext<BookingStatusHub> hubContext,
        ILogger<BookingNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendBookingUpdateAsync(Guid bookingId, object updateData)
    {
        await _hubContext.Clients
            .Group($"booking-{bookingId}")
            .SendAsync("booking-update", new
            {
                bookingId = bookingId,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("Booking update sent for booking {BookingId}", bookingId);
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

