using ReportingService.Domain.Entities;
using ReportingService.Application.DTOs.Responses.Report;
using SharedKernel.Data;

namespace ReportingService.Application.Interfaces;

/// <summary>
/// Repository interface for reporting operations
/// </summary>
public interface IReportingRepository : IBaseRepository<ReportSchedule, string>
{
    /// <summary>
    /// Get booking statistics for a date range
    /// </summary>
    Task<BookingStats> GetBookingStatsAsync(string tenantId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get monthly revenue breakdown for a year
    /// </summary>
    Task<IEnumerable<MonthlyRevenue>> GetRevenueByMonthAsync(string tenantId, int year);

    /// <summary>
    /// Get top destinations by booking count
    /// </summary>
    Task<IEnumerable<TopDestination>> GetTopDestinationsAsync(string tenantId, int limit);
}
