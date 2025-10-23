namespace ReportingService.Contracts.Responses.Report;

public class ReportResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public List<string>? Columns { get; set; }
    public string? GroupBy { get; set; }
    public int Status { get; set; }
    public string? FilePath { get; set; }
    public string? Schedule { get; set; }
    public string? EmailRecipients { get; set; }
    public DateTime? LastGenerated { get; set; }
    public DateTime? NextScheduled { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
