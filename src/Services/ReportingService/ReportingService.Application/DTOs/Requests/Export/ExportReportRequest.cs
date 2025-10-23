namespace ReportingService.Application.DTOs.Requests.Export;

public class ExportReportRequest
{
    public Guid ReportId { get; set; }
    public string ExportFormat { get; set; } = "PDF";
    public Dictionary<string, object>? ExportOptions { get; set; }
}
