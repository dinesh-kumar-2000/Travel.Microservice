using System.ComponentModel.DataAnnotations;

namespace ReportingService.Contracts.Requests.Report;

public class GenerateReportRequest
{
    [Required]
    [MinLength(5)]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    public int ReportType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public Dictionary<string, object>? Filters { get; set; }
    public List<string>? Columns { get; set; }
    public string? GroupBy { get; set; }
}
