namespace NotificationService.Contracts.Requests.PushNotification;

public class SendBulkPushNotificationRequest
{
    public string[] DeviceTokens { get; set; } = Array.Empty<string>();
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Data { get; set; }
    public string? ImageUrl { get; set; }
}