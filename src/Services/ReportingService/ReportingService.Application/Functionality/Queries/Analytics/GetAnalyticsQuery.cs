using MediatR;
using ReportingService.Application.DTOs.Responses.Analytics;

namespace ReportingService.Application.Queries.Analytics;

public class GetAnalyticsQuery : IRequest<AnalyticsResponse?>
{
    public Guid AnalyticsId { get; set; }
}
