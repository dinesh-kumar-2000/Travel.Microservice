using MediatR;
using SharedKernel.Models;
using NotificationService.Contracts.Responses.Notification;

namespace NotificationService.Application.Queries.Notification;

public class GetUserNotificationsQuery : IRequest<PaginatedResult<NotificationResponse>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
