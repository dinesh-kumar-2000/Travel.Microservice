using MediatR;
using ReportingService.Application.DTOs.Responses.Dashboard;

namespace ReportingService.Application.Queries.Dashboard;

public class GetDashboardQuery : IRequest<DashboardResponse?>
{
    public Guid DashboardId { get; set; }
}
