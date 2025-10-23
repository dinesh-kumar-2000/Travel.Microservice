namespace ReportingService.Contracts.Responses.Analytics;

public class AnalyticsDataResponse
{
    public Guid AnalyticsId { get; set; }
    public List<AnalyticsDataPoint> DataPoints { get; set; } = new();
    public Dictionary<string, object>? Summary { get; set; }
    public List<string>? Labels { get; set; }
    public List<object>? Values { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class AnalyticsDataPoint
{
    public string Label { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}
