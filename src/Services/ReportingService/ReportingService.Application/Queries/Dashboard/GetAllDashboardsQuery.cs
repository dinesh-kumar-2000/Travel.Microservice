using MediatR;
using SharedKernel.Models;
using ReportingService.Contracts.Responses.Dashboard;

namespace ReportingService.Application.Queries.Dashboard;

public class GetAllDashboardsQuery : IRequest<PaginatedResult<DashboardResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
