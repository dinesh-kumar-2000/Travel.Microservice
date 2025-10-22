using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.RateLimiting;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Creates standard fixed window rate limiter options
    /// </summary>
    private static FixedWindowRateLimiterOptions CreateFixedWindowOptions(
        int permitLimit, 
        int queueLimit = 5)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = queueLimit
        };
    }

    public static IServiceCollection AddTenantRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Add named "fixed" policy
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                var standardOptions = CreateFixedWindowOptions(100);
                opt.PermitLimit = standardOptions.PermitLimit;
                opt.Window = standardOptions.Window;
                opt.QueueProcessingOrder = standardOptions.QueueProcessingOrder;
                opt.QueueLimit = standardOptions.QueueLimit;
            });

            // Global rate limit
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();

                if (string.IsNullOrEmpty(tenantId))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        "anonymous", 
                        _ => CreateFixedWindowOptions(permitLimit: 10, queueLimit: 0));
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    tenantId, 
                    _ => CreateFixedWindowOptions(permitLimit: 100, queueLimit: 5));
            });

            options.RejectionStatusCode = 429;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync(
                    "Rate limit exceeded. Please try again later.",
                    cancellationToken);
            };
        });

        return services;
    }
}

