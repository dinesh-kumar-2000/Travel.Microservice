namespace ReportingService.Application.DTOs.Requests.Dashboard;

public class CreateDashboardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DashboardType { get; set; }
    public Dictionary<string, object>? Layout { get; set; }
    public List<string>? Widgets { get; set; }
    public bool IsPublic { get; set; } = false;
}
