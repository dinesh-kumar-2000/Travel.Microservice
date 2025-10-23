using MediatR;
using SharedKernel.Models;
using NotificationService.Contracts.Responses.Notification;

namespace NotificationService.Application.Queries.Notification;

public class GetAllNotificationsQuery : IRequest<PaginatedResult<NotificationResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
