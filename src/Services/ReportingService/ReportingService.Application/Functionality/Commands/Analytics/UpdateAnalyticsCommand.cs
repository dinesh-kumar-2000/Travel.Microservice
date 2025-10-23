using MediatR;
using ReportingService.Application.DTOs.Responses.Analytics;

namespace ReportingService.Application.Commands.Analytics;

public class UpdateAnalyticsCommand : IRequest<AnalyticsResponse>
{
    public Guid AnalyticsId { get; set; }
    public int AnalyticsType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public string? GroupBy { get; set; }
}
