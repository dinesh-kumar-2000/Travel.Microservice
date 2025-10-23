using MediatR;
using NotificationService.Application.DTOs.Responses.Notification;

namespace NotificationService.Application.Queries.Notification;

public class GetNotificationQuery : IRequest<NotificationResponse?>
{
    public Guid NotificationId { get; set; }
}
