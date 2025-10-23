using SharedKernel.Models;

namespace ReviewsService.Domain.Entities;

public class Review : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = new();
    public string Status { get; set; } = "pending";
    public string? ModerationNotes { get; set; }
    public string? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public void Publish()
    {
        Status = "published";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string moderatorId, string notes)
    {
        Status = "rejected";
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Moderate(string moderatorId, string newStatus, string? notes)
    {
        Status = newStatus;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValidRating()
    {
        return Rating >= 1 && Rating <= 5;
    }

    public void IncrementHelpful()
    {
        HelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementNotHelpful()
    {
        NotHelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ReviewVote : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string ReviewId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Review? Review { get; set; }
}

public class ReviewResponse : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string ReviewId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ResponderId { get; set; } = string.Empty;
    public string ResponderName { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Review? Review { get; set; }
}

