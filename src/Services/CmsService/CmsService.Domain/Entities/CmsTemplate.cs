using SharedKernel.Models;

namespace CmsService.Domain.Entities;

public class CmsTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public string? TemplateType { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<CmsContent> Contents { get; set; } = new List<CmsContent>();
    public virtual ICollection<CmsPage> Pages { get; set; } = new List<CmsPage>();
}
