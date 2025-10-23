using MediatR;
using ReportingService.Contracts.Responses.Dashboard;

namespace ReportingService.Application.Commands.Dashboard;

public class CreateDashboardCommand : IRequest<DashboardResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DashboardType { get; set; }
    public Dictionary<string, object>? Layout { get; set; }
    public List<string>? Widgets { get; set; }
    public bool IsPublic { get; set; } = false;
}
