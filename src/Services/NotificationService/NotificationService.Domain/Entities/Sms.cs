using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class Sms : BaseEntity<string>
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public int Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsActive { get; set; } = true;
}
