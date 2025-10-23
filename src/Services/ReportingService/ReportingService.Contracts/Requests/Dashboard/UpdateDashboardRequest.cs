namespace ReportingService.Contracts.Requests.Dashboard;

public class UpdateDashboardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DashboardType { get; set; }
    public Dictionary<string, object>? Layout { get; set; }
    public List<string>? Widgets { get; set; }
    public bool IsPublic { get; set; } = false;
}
