using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class Notification : TenantEntity<string>
{
    public string RecipientId { get; private set; } = string.Empty;
    public string RecipientEmail { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public DateTime? SentAt { get; private set; }

    private Notification() { }

    public static Notification Create(string id, string tenantId, string recipientId, 
        string recipientEmail, NotificationType type, string subject, string body)
    {
        return new Notification
        {
            Id = id,
            TenantId = tenantId,
            RecipientId = recipientId,
            RecipientEmail = recipientEmail,
            Type = type,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum NotificationType
{
    Email,
    SMS,
    Push
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}

