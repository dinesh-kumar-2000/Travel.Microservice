using MediatR;
using SharedKernel.Models;
using ReportingService.Contracts.Responses.Report;

namespace ReportingService.Application.Queries.Report;

public class GetAllReportsQuery : IRequest<PaginatedResult<ReportResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
