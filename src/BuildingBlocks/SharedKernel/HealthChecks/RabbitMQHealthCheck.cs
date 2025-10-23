using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MassTransit;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Health check for RabbitMQ message broker connectivity
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IBus _bus;
    private readonly ILogger<RabbitMQHealthCheck> _logger;

    public RabbitMQHealthCheck(IBus bus, ILogger<RabbitMQHealthCheck> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test message publishing capability
            var testMessage = new TestHealthMessage { Timestamp = DateTime.UtcNow };
            
            var startTime = DateTime.UtcNow;
            await _bus.Publish(testMessage, cancellationToken);
            var responseTime = DateTime.UtcNow - startTime;
            
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = responseTime.TotalMilliseconds,
                ["bus_state"] = "Connected",
                ["timestamp"] = DateTime.UtcNow
            };
            
            return HealthCheckResult.Healthy("RabbitMQ is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed");
            return HealthCheckResult.Unhealthy("RabbitMQ is unavailable", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["bus_state"] = "Disconnected",
                ["timestamp"] = DateTime.UtcNow
            });
        }
    }
}

/// <summary>
/// Test message for health check
/// </summary>
public class TestHealthMessage
{
    public DateTime Timestamp { get; set; }
}
