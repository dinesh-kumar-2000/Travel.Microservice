using SharedKernel.Models;

namespace ReportingService.Domain.Entities;

public class Dashboard : BaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DashboardType { get; set; }
    public Dictionary<string, object>? Layout { get; set; }
    public List<string>? Widgets { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
