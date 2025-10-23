using NotificationService.Application.DTOs.Responses.PushNotification;

namespace NotificationService.Application.DTOs.Responses.PushNotification;

public class PushNotificationHistoryResponse
{
    public List<PushNotificationResponse> PushNotifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class BulkPushNotificationResponse
{
    public int TotalSent { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<Guid> FailedUserIds { get; set; } = new();
    public List<PushNotificationResponse> PushNotificationResponses { get; set; } = new();
}
