using MediatR;
using NotificationService.Application.DTOs.Responses.PushNotification;

namespace NotificationService.Application.Commands.PushNotification;

public class SendBulkPushNotificationCommand : IRequest<BulkPushNotificationResponse>
{
    public List<Guid> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
}
