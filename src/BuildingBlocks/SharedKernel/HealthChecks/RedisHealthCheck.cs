using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity and performance
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            
            // Test basic connectivity with ping
            var startTime = DateTime.UtcNow;
            var pingResult = await database.PingAsync();
            var responseTime = DateTime.UtcNow - startTime;
            
            // Check if response time is acceptable (less than 50ms)
            if (responseTime.TotalMilliseconds > 50)
            {
                return HealthCheckResult.Degraded(
                    $"Redis response time is slow: {responseTime.TotalMilliseconds}ms",
                    data: new Dictionary<string, object>
                    {
                        ["response_time_ms"] = responseTime.TotalMilliseconds,
                        ["threshold_ms"] = 50
                    });
            }
            
            // Test memory usage
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var info = await server.InfoAsync("memory");
            var usedMemoryGroup = info.FirstOrDefault(x => x.Key == "used_memory");
            var usedMemory = usedMemoryGroup?.FirstOrDefault().Value;
            
            // Test key operations
            var testKey = $"health_check_{Guid.NewGuid()}";
            await database.StringSetAsync(testKey, "test", TimeSpan.FromSeconds(10));
            var retrievedValue = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);
            
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = responseTime.TotalMilliseconds,
                ["ping_ms"] = pingResult.TotalMilliseconds,
                ["used_memory"] = usedMemory ?? "unknown",
                ["connection_state"] = _redis.GetStatus()
            };
            
            return HealthCheckResult.Healthy("Redis is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis is unavailable", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["connection_state"] = _redis.GetStatus(),
                ["timestamp"] = DateTime.UtcNow
            });
        }
    }
}
