using SharedKernel.Models;

namespace CmsService.Domain.Entities;

public class BlogPost : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public string? FeaturedImage { get; set; }
    public string Status { get; set; } = "draft";
    public int Views { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public void Publish()
    {
        Status = "published";
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = "archived";
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViews()
    {
        Views++;
    }
}

