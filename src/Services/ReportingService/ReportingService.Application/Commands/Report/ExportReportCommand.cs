using MediatR;
using ReportingService.Contracts.Responses.Report;

namespace ReportingService.Application.Commands.Report;

public class ExportReportCommand : IRequest<ExportResponse>
{
    public Guid ReportId { get; set; }
    public string ExportFormat { get; set; } = string.Empty;
    public Dictionary<string, object>? ExportOptions { get; set; }
}
