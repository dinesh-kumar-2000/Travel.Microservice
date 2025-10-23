using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class Email : BaseEntity<string>
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public List<string>? Attachments { get; set; }
    public int Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsActive { get; set; } = true;
}
