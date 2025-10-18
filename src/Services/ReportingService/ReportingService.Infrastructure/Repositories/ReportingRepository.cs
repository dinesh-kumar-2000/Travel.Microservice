using Dapper;
using SharedKernel.Caching;
using System.Data;
using Npgsql;

namespace ReportingService.Infrastructure.Repositories;

public interface IReportingRepository
{
    Task<BookingStats> GetBookingStatsAsync(string tenantId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MonthlyRevenue>> GetRevenueByMonthAsync(string tenantId, int year);
    Task<IEnumerable<TopDestination>> GetTopDestinationsAsync(string tenantId, int limit);
}

public class ReportingRepository : IReportingRepository
{
    private readonly string _connectionString;
    private readonly ICacheService _cache;

    public ReportingRepository(string connectionString, ICacheService cache)
    {
        _connectionString = connectionString;
        _cache = cache;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<BookingStats> GetBookingStatsAsync(string tenantId, DateTime startDate, DateTime endDate)
    {
        var cacheKey = $"stats:booking:{tenantId}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            
            // Use database function
            const string sql = "SELECT * FROM fn_get_booking_stats(@TenantId, @StartDate, @EndDate)";
            
            return await connection.QueryFirstOrDefaultAsync<BookingStats>(sql, new 
            { 
                TenantId = tenantId, 
                StartDate = startDate, 
                EndDate = endDate 
            }) ?? new BookingStats();
        }, TimeSpan.FromMinutes(15));
    }

    public async Task<IEnumerable<MonthlyRevenue>> GetRevenueByMonthAsync(string tenantId, int year)
    {
        var cacheKey = $"stats:revenue:monthly:{tenantId}:{year}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            
            // Use database function
            const string sql = "SELECT * FROM fn_get_revenue_by_month(@TenantId, @Year)";
            
            var result = await connection.QueryAsync<MonthlyRevenue>(sql, new 
            { 
                TenantId = tenantId, 
                Year = year 
            });
            
            return result.ToList();
        }, TimeSpan.FromHours(1));
    }

    public async Task<IEnumerable<TopDestination>> GetTopDestinationsAsync(string tenantId, int limit)
    {
        var cacheKey = $"stats:destinations:top:{tenantId}:{limit}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            
            // Use database function
            const string sql = "SELECT * FROM fn_get_top_destinations(@TenantId, @Limit)";
            
            var result = await connection.QueryAsync<TopDestination>(sql, new 
            { 
                TenantId = tenantId, 
                Limit = limit 
            });
            
            return result.ToList();
        }, TimeSpan.FromMinutes(30));
    }
}

public class BookingStats
{
    public long TotalBookings { get; set; }
    public long ConfirmedBookings { get; set; }
    public long CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int TotalTravelers { get; set; }
}

public class MonthlyRevenue
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public long TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public long TotalTravelers { get; set; }
}

public class TopDestination
{
    public string Destination { get; set; } = string.Empty;
    public long BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public long TotalTravelers { get; set; }
    public decimal AverageBookingValue { get; set; }
}

