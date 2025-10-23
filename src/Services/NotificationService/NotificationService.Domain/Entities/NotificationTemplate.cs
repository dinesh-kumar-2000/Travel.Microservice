using SharedKernel.Models;

namespace NotificationService.Domain.Entities;

public class NotificationTemplate : BaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int TemplateType { get; set; }
    public string Language { get; set; } = "en";
    public Dictionary<string, object>? Variables { get; set; }
    public bool IsActive { get; set; } = true;
}
