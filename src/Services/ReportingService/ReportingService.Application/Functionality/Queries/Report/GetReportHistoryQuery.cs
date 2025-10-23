using MediatR;
using SharedKernel.Models;
using ReportingService.Application.DTOs.Responses.Report;

namespace ReportingService.Application.Queries.Report;

public class GetReportHistoryQuery : IRequest<PaginatedResult<ReportResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
