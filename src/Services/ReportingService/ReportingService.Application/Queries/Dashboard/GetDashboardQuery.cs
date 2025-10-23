using MediatR;
using ReportingService.Contracts.Responses.Dashboard;

namespace ReportingService.Application.Queries.Dashboard;

public class GetDashboardQuery : IRequest<DashboardResponse?>
{
    public Guid DashboardId { get; set; }
}
