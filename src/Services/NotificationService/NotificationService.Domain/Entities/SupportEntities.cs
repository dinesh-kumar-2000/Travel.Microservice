namespace NotificationService.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public string Status { get; set; } = "open";
    public Guid? BookingId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? SatisfactionFeedback { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastResponseAt { get; set; }

    // Navigation
    public List<SupportMessage> Messages { get; set; } = new();
    public List<SupportTicketTag> Tags { get; set; } = new();

    public void Assign(Guid agentId)
    {
        AssignedTo = agentId;
        AssignedAt = DateTime.UtcNow;
        Status = "in-progress";
    }

    public void Resolve(string notes)
    {
        Status = "resolved";
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = notes;
    }

    public void Close(int? rating, string? feedback)
    {
        Status = "closed";
        ClosedAt = DateTime.UtcNow;
        SatisfactionRating = rating;
        SatisfactionFeedback = feedback;
    }

    public void Reopen()
    {
        Status = "open";
        ResolvedAt = null;
        ClosedAt = null;
    }

    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
    }

    public bool CanBeClosedByUser(Guid userId)
    {
        return UserId == userId && Status is "open" or "in-progress" or "waiting-response" or "resolved";
    }
}

public class SupportMessage
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty; // user, agent, system
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Attachments { get; set; }
    public bool IsInternalNote { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public SupportTicket? Ticket { get; set; }

    public void MarkAsRead()
    {
        ReadAt = DateTime.UtcNow;
    }

    public bool IsUnread()
    {
        return ReadAt == null;
    }
}

public class SupportTicketTag
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Tag { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public SupportTicket? Ticket { get; set; }
}

public class SupportCannedResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Shortcut { get; set; }
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void IncrementUsage()
    {
        UsageCount++;
    }
}

public class SupportSLAConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int FirstResponseTimeMinutes { get; set; }
    public int ResolutionTimeHours { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SupportTicketActivity
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public SupportTicket? Ticket { get; set; }
}

