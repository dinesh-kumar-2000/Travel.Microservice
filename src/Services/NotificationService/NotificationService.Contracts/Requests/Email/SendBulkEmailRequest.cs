namespace NotificationService.Contracts.Requests.Email;

public class SendBulkEmailRequest
{
    public string[] To { get; set; } = Array.Empty<string>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string[]? Attachments { get; set; }
}