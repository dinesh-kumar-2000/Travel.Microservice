namespace ReportingService.Application.DTOs.Responses.Report;

/// <summary>
/// Top destination response
/// </summary>
public class TopDestination
{
    public string DestinationId { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
}
