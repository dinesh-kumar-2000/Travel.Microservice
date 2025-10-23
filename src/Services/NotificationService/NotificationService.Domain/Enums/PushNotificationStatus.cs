namespace NotificationService.Domain.Enums;

public enum PushNotificationStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}
