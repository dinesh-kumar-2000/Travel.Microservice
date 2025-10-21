namespace BookingService.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid BookingId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = new();
    public string Status { get; set; } = "pending";
    public string? ModerationNotes { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void Publish()
    {
        Status = "published";
    }

    public void Reject(Guid moderatorId, string notes)
    {
        Status = "rejected";
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
    }

    public void Moderate(Guid moderatorId, string newStatus, string? notes)
    {
        Status = newStatus;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
    }

    public bool IsValidRating()
    {
        return Rating >= 1 && Rating <= 5;
    }
}

public class ReviewVote
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsHelpful { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Review? Review { get; set; }
}

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ResponderId { get; set; }
    public string ResponderName { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Review? Review { get; set; }
}

