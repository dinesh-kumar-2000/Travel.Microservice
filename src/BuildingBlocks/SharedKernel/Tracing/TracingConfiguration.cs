using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace SharedKernel.Tracing;

/// <summary>
/// OpenTelemetry configuration for distributed tracing and metrics
/// </summary>
public static class TracingConfiguration
{
    /// <summary>
    /// Configures OpenTelemetry for a service
    /// </summary>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        string? jaegerEndpoint = null,
        string? zipkinEndpoint = null,
        bool enableConsoleExporter = false)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ["service.instance.id"] = Environment.MachineName
            });

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                            activity.SetTag("user_agent.original", httpRequest.Headers.UserAgent.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.request.method", httpRequestMessage.Method.Method);
                            activity.SetTag("http.request.url", httpRequestMessage.RequestUri?.ToString());
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource(serviceName);

                // Add exporters
                if (!string.IsNullOrEmpty(jaegerEndpoint))
                {
                    // Jaeger exporter configuration would go here
                }

                if (!string.IsNullOrEmpty(zipkinEndpoint))
                {
                    // Zipkin exporter configuration would go here
                }

                if (enableConsoleExporter)
                {
                    // Console exporter configuration would go here
                }
            })
            .WithMetrics(metricsProviderBuilder =>
            {
                metricsProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter(serviceName)
                    .AddPrometheusExporter();
            });

        return services;
    }

    /// <summary>
    /// Adds custom activity sources for business operations
    /// </summary>
    public static IServiceCollection AddBusinessTracing(
        this IServiceCollection services,
        params string[] activitySourceNames)
    {
        foreach (var sourceName in activitySourceNames)
        {
            services.AddSingleton(new ActivitySource(sourceName));
        }

        return services;
    }

    /// <summary>
    /// Configures tracing for development environment
    /// </summary>
    public static IServiceCollection AddDevelopmentTracing(
        this IServiceCollection services,
        string serviceName)
    {
        return services.AddOpenTelemetryTracing(
            serviceName,
            GetAssemblyVersion(),
            enableConsoleExporter: true);
    }

    /// <summary>
    /// Configures tracing for production environment
    /// </summary>
    public static IServiceCollection AddProductionTracing(
        this IServiceCollection services,
        string serviceName,
        string? jaegerEndpoint = null,
        string? zipkinEndpoint = null)
    {
        return services.AddOpenTelemetryTracing(
            serviceName,
            GetAssemblyVersion(),
            jaegerEndpoint,
            zipkinEndpoint);
    }

    private static string GetAssemblyVersion()
    {
        try
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        }
        catch
        {
            return "1.0.0.0";
        }
    }
}
