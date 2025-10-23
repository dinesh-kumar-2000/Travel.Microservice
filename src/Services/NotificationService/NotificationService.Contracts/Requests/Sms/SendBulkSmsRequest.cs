namespace NotificationService.Contracts.Requests.Sms;

public class SendBulkSmsRequest
{
    public string[] To { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
    public string? SenderId { get; set; }
}