namespace ReportingService.Application.DTOs.Requests.Analytics;

public class GenerateAnalyticsRequest
{
    public int AnalyticsType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public string? GroupBy { get; set; }
}
