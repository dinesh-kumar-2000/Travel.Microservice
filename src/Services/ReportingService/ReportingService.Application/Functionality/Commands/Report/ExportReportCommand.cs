using MediatR;
using ReportingService.Application.DTOs.Responses.Report;

namespace ReportingService.Application.Commands.Report;

public class ExportReportCommand : IRequest<ExportResponse>
{
    public Guid ReportId { get; set; }
    public string ExportFormat { get; set; } = string.Empty;
    public Dictionary<string, object>? ExportOptions { get; set; }
}
