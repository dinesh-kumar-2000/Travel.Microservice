using System.ComponentModel.DataAnnotations;

namespace ReportingService.Application.DTOs.Requests.Report;

public class ExportReportRequest
{
    [Required]
    public Guid ReportId { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(10)]
    public string ExportFormat { get; set; } = string.Empty;

    public Dictionary<string, object>? ExportOptions { get; set; }
}
