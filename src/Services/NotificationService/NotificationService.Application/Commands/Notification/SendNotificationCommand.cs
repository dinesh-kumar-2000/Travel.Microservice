using MediatR;
using NotificationService.Contracts.Responses.Notification;

namespace NotificationService.Application.Commands.Notification;

public class SendNotificationCommand : IRequest<NotificationResponse>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int NotificationType { get; set; }
    public int Priority { get; set; } = 1;
    public Dictionary<string, object>? Data { get; set; }
}
