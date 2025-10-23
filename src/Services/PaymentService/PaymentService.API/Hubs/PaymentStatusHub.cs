using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PaymentService.API.Hubs;

[Authorize]
public class PaymentStatusHub : Hub
{
    private readonly ILogger<PaymentStatusHub> _logger;

    public PaymentStatusHub(ILogger<PaymentStatusHub> logger)
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
            _logger.LogInformation("User {UserId} connected to paymentservice status hub", userId);
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
            _logger.LogInformation("User {UserId} disconnected from paymentservice status hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to paymentservice updates
    /// </summary>
    public async Task SubscribeToPayment(string updateType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"paymentservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} subscribed to paymentservice {UpdateType}",
            Context.ConnectionId, updateType);
    }

    /// <summary>
    /// Unsubscribe from paymentservice updates
    /// </summary>
    public async Task UnsubscribeFromPayment(string updateType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"paymentservice-{updateType}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from paymentservice {UpdateType}",
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
/// Service for sending paymentservice updates to connected clients
/// </summary>
public interface IPaymentNotificationService
{
    Task SendPaymentUpdateAsync(string updateType, object updateData);
    Task SendUserNotificationAsync(Guid userId, object notification);
}

public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly IHubContext<PaymentStatusHub> _hubContext;
    private readonly ILogger<PaymentNotificationService> _logger;

    public PaymentNotificationService(
        IHubContext<PaymentStatusHub> hubContext,
        ILogger<PaymentNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendPaymentUpdateAsync(string updateType, object updateData)
    {
        await _hubContext.Clients
            .Group($"paymentservice-{updateType}")
            .SendAsync("paymentservice-update", new
            {
                updateType = updateType,
                data = updateData,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("paymentservice update sent for type {UpdateType}", updateType);
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
