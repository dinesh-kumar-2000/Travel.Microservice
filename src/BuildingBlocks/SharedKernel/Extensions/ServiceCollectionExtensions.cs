using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Auditing;
using SharedKernel.Behaviors;
using SharedKernel.Caching;
using SharedKernel.RateLimiting;
using SharedKernel.Versioning;
using SharedKernel.Utilities;

namespace SharedKernel.Extensions;

/// <summary>
/// Common service registration extensions to eliminate duplicate setup code across services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds common infrastructure services (correlation ID, cache, audit, versioning, rate limiting)
    /// Note: Multi-tenancy setup should be added separately as it's in a separate assembly
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        string? redisConnection = null)
    {
        // HTTP Context Accessor
        services.AddHttpContextAccessor();

        // ID and DateTime providers
        services.AddSingleton<IIdGenerator, UlidIdGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Correlation ID
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

        // Distributed Cache (Redis)
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "TravelPortal:";
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        // Audit Service
        services.AddScoped<IAuditService, AuditService>();

        // API Versioning
        services.AddApiVersioningConfiguration();

        // Rate Limiting
        services.AddTenantRateLimiting();

        return services;
    }

    /// <summary>
    /// Adds standard middleware pipeline for services
    /// Note: Multi-tenancy middleware should be added separately
    /// </summary>
    public static IApplicationBuilder UseCommonMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<Middleware.CorrelationIdMiddleware>();
        app.UseMiddleware<Middleware.GlobalExceptionHandlingMiddleware>();
        
        return app;
    }
}

