using MediatR;

namespace ReportingService.Application.Commands.Dashboard;

public class DeleteDashboardCommand : IRequest<Unit>
{
    public Guid DashboardId { get; set; }
}
