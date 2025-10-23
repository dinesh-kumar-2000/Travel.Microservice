using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using System.Data;
using Dapper;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Health check for database connectivity and performance
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDapperContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDapperContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Test basic connectivity
            connection.Open();
            
            // Test query performance
            var startTime = DateTime.UtcNow;
            var result = await connection.QuerySingleAsync<int>("SELECT 1");
            var responseTime = DateTime.UtcNow - startTime;
            
            // Check if response time is acceptable (less than 100ms)
            if (responseTime.TotalMilliseconds > 100)
            {
                return HealthCheckResult.Degraded(
                    $"Database response time is slow: {responseTime.TotalMilliseconds}ms",
                    data: new Dictionary<string, object>
                    {
                        ["response_time_ms"] = responseTime.TotalMilliseconds,
                        ["threshold_ms"] = 100
                    });
            }
            
            // Test database size and connection pool
            var connectionCount = await connection.QuerySingleAsync<int>(
                "SELECT count(*) FROM pg_stat_activity WHERE state = 'active'");
            
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = responseTime.TotalMilliseconds,
                ["active_connections"] = connectionCount,
                ["database"] = connection.Database ?? "unknown"
            };
            
            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unavailable", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });
        }
    }
}
