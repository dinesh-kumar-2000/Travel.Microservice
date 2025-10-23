using MediatR;
using ReportingService.Application.DTOs.Responses.Analytics;

namespace ReportingService.Application.Queries.Analytics;

public class GetAnalyticsDataQuery : IRequest<AnalyticsDataResponse?>
{
    public Guid AnalyticsId { get; set; }
}
