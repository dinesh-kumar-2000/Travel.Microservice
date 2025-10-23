using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SharedKernel.SignalR;

public interface INotificationService
{
    Task SendNotificationToUserAsync(string userId, string message, string? type = null);
    Task SendNotificationToGroupAsync(string groupName, string message, string? type = null);
    Task SendNotificationToAllAsync(string message, string? type = null);
    Task SendNotificationToAllExceptAsync(string message, IReadOnlyList<string> excludedUserIds, string? type = null);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(string userId, string message, string? type = null)
    {
        try
        {
            await _hubContext.SendToUserAsync(userId, "ReceiveNotification", new { message, type, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
        }
    }

    public async Task SendNotificationToGroupAsync(string groupName, string message, string? type = null)
    {
        try
        {
            await _hubContext.SendToGroupAsync(groupName, "ReceiveNotification", new { message, type, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Notification sent to group {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to group {GroupName}", groupName);
        }
    }

    public async Task SendNotificationToAllAsync(string message, string? type = null)
    {
        try
        {
            await _hubContext.SendToAllAsync("ReceiveNotification", new { message, type, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Notification sent to all users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to all users");
        }
    }

    public async Task SendNotificationToAllExceptAsync(string message, IReadOnlyList<string> excludedUserIds, string? type = null)
    {
        try
        {
            await _hubContext.SendToAllExceptAsync("ReceiveNotification", excludedUserIds, new { message, type, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Notification sent to all users except {ExcludedCount} users", excludedUserIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to all users except specified exclusions");
        }
    }
}
