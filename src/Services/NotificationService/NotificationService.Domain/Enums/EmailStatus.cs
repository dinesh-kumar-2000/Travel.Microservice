namespace NotificationService.Domain.Enums;

public enum EmailStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Bounced = 4,
    Failed = 5,
    Opened = 6,
    Clicked = 7
}
