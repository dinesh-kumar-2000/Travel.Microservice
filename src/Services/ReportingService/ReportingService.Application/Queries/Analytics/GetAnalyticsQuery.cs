using MediatR;
using ReportingService.Contracts.Responses.Analytics;

namespace ReportingService.Application.Queries.Analytics;

public class GetAnalyticsQuery : IRequest<AnalyticsResponse?>
{
    public Guid AnalyticsId { get; set; }
}
