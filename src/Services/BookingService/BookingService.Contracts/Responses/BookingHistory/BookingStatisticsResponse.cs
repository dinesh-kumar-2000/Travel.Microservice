namespace BookingService.Contracts.Responses.BookingHistory;

public class BookingStatisticsResponse
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<MonthlyStatistics> MonthlyStats { get; set; } = new();
    public List<ServiceTypeStatistics> ServiceTypeStats { get; set; } = new();
}

public class MonthlyStatistics
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class ServiceTypeStatistics
{
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}
