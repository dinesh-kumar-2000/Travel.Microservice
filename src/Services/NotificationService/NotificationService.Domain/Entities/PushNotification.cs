using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class PushNotification : BaseEntity<string>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public int Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsActive { get; set; } = true;
}
