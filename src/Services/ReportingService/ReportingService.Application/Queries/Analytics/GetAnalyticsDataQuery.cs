using MediatR;
using ReportingService.Contracts.Responses.Analytics;

namespace ReportingService.Application.Queries.Analytics;

public class GetAnalyticsDataQuery : IRequest<AnalyticsDataResponse?>
{
    public Guid AnalyticsId { get; set; }
}
