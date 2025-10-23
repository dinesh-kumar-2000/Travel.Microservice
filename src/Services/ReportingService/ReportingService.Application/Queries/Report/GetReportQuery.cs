using MediatR;
using ReportingService.Contracts.Responses.Report;

namespace ReportingService.Application.Queries.Report;

public class GetReportQuery : IRequest<ReportResponse?>
{
    public Guid ReportId { get; set; }
}
