namespace BookingService.Application.DTOs.Responses.BookingHistory;

public class BookingStatisticsResponseDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<MonthlyStatisticsDto> MonthlyStats { get; set; } = new();
    public List<ServiceTypeStatisticsDto> ServiceTypeStats { get; set; } = new();
}

public class MonthlyStatisticsDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class ServiceTypeStatisticsDto
{
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}
