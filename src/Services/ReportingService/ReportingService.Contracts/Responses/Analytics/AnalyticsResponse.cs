namespace ReportingService.Contracts.Responses.Analytics;

public class AnalyticsResponse
{
    public Guid Id { get; set; }
    public int AnalyticsType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public string? GroupBy { get; set; }
    public int Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
