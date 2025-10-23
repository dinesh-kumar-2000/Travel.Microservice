namespace ReportingService.Application.DTOs.Responses.Report;

/// <summary>
/// Monthly revenue response
/// </summary>
public class MonthlyRevenue
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public int Year { get; set; }
}
