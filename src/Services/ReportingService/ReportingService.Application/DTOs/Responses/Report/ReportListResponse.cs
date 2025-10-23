using ReportingService.Application.DTOs.Responses.Report;

namespace ReportingService.Application.DTOs.Responses.Report;

public class ReportListResponse
{
    public List<ReportResponse> Reports { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ExportResponse
{
    public Guid ReportId { get; set; }
    public string ExportFormat { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
}
