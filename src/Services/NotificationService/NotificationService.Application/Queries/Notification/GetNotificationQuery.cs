using MediatR;
using NotificationService.Contracts.Responses.Notification;

namespace NotificationService.Application.Queries.Notification;

public class GetNotificationQuery : IRequest<NotificationResponse?>
{
    public Guid NotificationId { get; set; }
}
