using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SharedKernel.SignalR;

public interface IRealTimeService
{
    Task SendMessageAsync(string userId, string message);
    Task SendMessageToGroupAsync(string groupName, string message);
    Task SendMessageToAllAsync(string message);
    Task JoinGroupAsync(string connectionId, string groupName);
    Task LeaveGroupAsync(string connectionId, string groupName);
    Task SendTypingIndicatorAsync(string userId, bool isTyping);
    Task SendUserStatusAsync(string userId, string status);
}

public class RealTimeService : IRealTimeService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealTimeService> _logger;

    public RealTimeService(IHubContext<NotificationHub> hubContext, ILogger<RealTimeService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendMessageAsync(string userId, string message)
    {
        try
        {
            await _hubContext.SendToUserAsync(userId, "ReceiveMessage", new { message, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Message sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to user {UserId}", userId);
        }
    }

    public async Task SendMessageToGroupAsync(string groupName, string message)
    {
        try
        {
            await _hubContext.SendToGroupAsync(groupName, "ReceiveMessage", new { message, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Message sent to group {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to group {GroupName}", groupName);
        }
    }

    public async Task SendMessageToAllAsync(string message)
    {
        try
        {
            await _hubContext.SendToAllAsync("ReceiveMessage", new { message, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Message sent to all users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to all users");
        }
    }

    public async Task JoinGroupAsync(string connectionId, string groupName)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined group {GroupName}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add connection {ConnectionId} to group {GroupName}", connectionId, groupName);
        }
    }

    public async Task LeaveGroupAsync(string connectionId, string groupName)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove connection {ConnectionId} from group {GroupName}", connectionId, groupName);
        }
    }

    public async Task SendTypingIndicatorAsync(string userId, bool isTyping)
    {
        try
        {
            await _hubContext.SendToUserAsync(userId, "UserTyping", new { userId, isTyping, timestamp = DateTime.UtcNow });
            _logger.LogInformation("Typing indicator sent for user {UserId}: {IsTyping}", userId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send typing indicator for user {UserId}", userId);
        }
    }

    public async Task SendUserStatusAsync(string userId, string status)
    {
        try
        {
            await _hubContext.SendToUserAsync(userId, "UserStatusChanged", new { userId, status, timestamp = DateTime.UtcNow });
            _logger.LogInformation("User status sent for user {UserId}: {Status}", userId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send user status for user {UserId}", userId);
        }
    }
}
