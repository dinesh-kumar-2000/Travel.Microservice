using MediatR;
using ReportingService.Contracts.Responses.Report;

namespace ReportingService.Application.Commands.Report;

public class ScheduleReportCommand : IRequest<ReportResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public List<string>? Columns { get; set; }
    public string? GroupBy { get; set; }
    public string Schedule { get; set; } = string.Empty;
    public string? EmailRecipients { get; set; }
}
