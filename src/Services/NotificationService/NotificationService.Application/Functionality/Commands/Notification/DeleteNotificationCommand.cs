using MediatR;

namespace NotificationService.Application.Commands.Notification;

public class DeleteNotificationCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
}
