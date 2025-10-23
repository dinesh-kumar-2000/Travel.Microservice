using MediatR;
using ReportingService.Application.DTOs.Responses.Report;

namespace ReportingService.Application.Commands.Report;

public class GenerateReportCommand : IRequest<ReportResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public List<string>? Columns { get; set; }
    public string? GroupBy { get; set; }
}
