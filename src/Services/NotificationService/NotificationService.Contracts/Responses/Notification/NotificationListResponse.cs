using NotificationService.Contracts.Responses.Notification;

namespace NotificationService.Contracts.Responses.Notification;

public class NotificationListResponse
{
    public List<NotificationResponse> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
