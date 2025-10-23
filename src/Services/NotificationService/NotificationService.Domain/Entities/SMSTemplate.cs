using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class SMSTemplate : TenantEntity<string>
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string RenderContent(Dictionary<string, string> values)
    {
        var rendered = Content;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return rendered;
    }

    public bool IsWithinSMSLimit()
    {
        return Content.Length <= 160; // Standard SMS length
    }
}

