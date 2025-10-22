using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel.Resilience;

namespace SharedKernel.Http;

/// <summary>
/// Factory for creating HttpClient with resilience policies
/// Eliminates duplication by centralizing policy handler configuration
/// </summary>
public static class ResilientHttpClientFactory
{
    /// <summary>
    /// Adds resilience policies (retry, circuit breaker, timeout) to an HttpClient builder
    /// </summary>
    private static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder builder,
        Func<IServiceProvider, ILogger> loggerFactory)
    {
        return builder
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = loggerFactory(serviceProvider);
                return ResiliencePolicies.GetRetryPolicy(logger);
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = loggerFactory(serviceProvider);
                return ResiliencePolicies.GetCircuitBreakerPolicy(logger);
            })
            .AddPolicyHandler(ResiliencePolicies.GetTimeoutPolicy());
    }

    /// <summary>
    /// Add typed HttpClient with retry and circuit breaker policies
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string name)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services.AddHttpClient<TClient, TImplementation>(name)
            .AddResiliencePolicies(sp => sp.GetRequiredService<ILogger<TImplementation>>())
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Add named HttpClient with resilience policies
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name)
    {
        return services.AddHttpClient(name)
            .AddResiliencePolicies(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger(name);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    }
}

