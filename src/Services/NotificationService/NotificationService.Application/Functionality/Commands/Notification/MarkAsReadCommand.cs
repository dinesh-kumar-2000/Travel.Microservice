using MediatR;

namespace NotificationService.Application.Commands.Notification;

public class MarkAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
}
