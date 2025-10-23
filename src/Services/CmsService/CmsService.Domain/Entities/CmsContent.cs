using SharedKernel.Models;

namespace CmsService.Domain.Entities;

public class CmsContent : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? TemplateId { get; set; }
    
    // Navigation properties
    public virtual CmsTemplate? Template { get; set; }
}
