using SharedKernel.Models;

namespace CmsService.Domain.Entities;

public class CmsPage : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string PageType { get; set; } = "custom";
    public string Status { get; set; } = "draft";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int SortOrder { get; set; }
    public string? ParentPageId { get; set; }
    public string? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public virtual CmsTemplate? Template { get; set; }
    public virtual CmsPage? ParentPage { get; set; }
    public virtual ICollection<CmsPage> ChildPages { get; set; } = new List<CmsPage>();

    public bool IsSystemPage()
    {
        return PageType is "terms" or "privacy" or "about" or "contact";
    }

    public void Publish()
    {
        Status = "published";
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
