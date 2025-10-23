namespace ReportingService.Application.DTOs.Responses.Dashboard;

public class DashboardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DashboardType { get; set; }
    public Dictionary<string, object>? Layout { get; set; }
    public List<string>? Widgets { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
