using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharedKernel.Data;
using StackExchange.Redis;
using MassTransit;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Extensions for registering health checks
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds comprehensive health checks for all infrastructure components
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services,
        string? redisConnection = null)
    {
        var healthChecksBuilder = services.AddHealthChecks();
        
        // Database health check
        healthChecksBuilder.AddCheck<DatabaseHealthCheck>(
            "database",
            tags: new[] { "db", "infrastructure" },
            timeout: TimeSpan.FromSeconds(10));
        
        // Redis health check (if configured)
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecksBuilder.AddCheck<RedisHealthCheck>(
                "redis",
                tags: new[] { "cache", "infrastructure" },
                timeout: TimeSpan.FromSeconds(5));
        }
        
        // RabbitMQ health check
        healthChecksBuilder.AddCheck<RabbitMQHealthCheck>(
            "rabbitmq",
            tags: new[] { "messaging", "infrastructure" },
            timeout: TimeSpan.FromSeconds(5));
        
        // System resources health check
        healthChecksBuilder.AddCheck<SystemResourcesHealthCheck>(
            "system_resources",
            tags: new[] { "system", "infrastructure" },
            timeout: TimeSpan.FromSeconds(5));
        
        // External services health check
        healthChecksBuilder.AddCheck<ExternalServiceHealthCheck>(
            "external_services",
            tags: new[] { "external", "infrastructure" },
            timeout: TimeSpan.FromSeconds(10));
        
        return services;
    }
    
    /// <summary>
    /// Adds custom business logic health checks
    /// </summary>
    public static IServiceCollection AddBusinessHealthChecks<TBusinessHealthCheck>(
        this IServiceCollection services,
        string checkName,
        string[]? tags = null)
        where TBusinessHealthCheck : class, IHealthCheck
    {
        services.AddHealthChecks()
            .AddCheck<TBusinessHealthCheck>(
                checkName,
                tags: tags ?? new[] { "business" },
                timeout: TimeSpan.FromSeconds(5));
        
        return services;
    }
    
    /// <summary>
    /// Adds health check UI for development
    /// </summary>
    public static IServiceCollection AddHealthCheckUI(this IServiceCollection services)
    {
        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(30);
            setup.MaximumHistoryEntriesPerEndpoint(50);
            setup.SetApiMaxActiveRequests(1);
            setup.AddHealthCheckEndpoint("Self", "/health");
        });
        
        return services;
    }
}
