using ReportingService.Application.DTOs.Responses.Dashboard;

namespace ReportingService.Application.DTOs.Responses.Dashboard;

public class DashboardListResponse
{
    public List<DashboardResponse> Dashboards { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
