using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel.Resilience;
using System.Net.Http.Json;

namespace SharedKernel.Http;

/// <summary>
/// Factory for creating HttpClient with resilience policies
/// </summary>
public static class ResilientHttpClientFactory
{
    /// <summary>
    /// Add HttpClient with retry and circuit breaker policies
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string name)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services.AddHttpClient<TClient, TImplementation>(name)
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TImplementation>>();
                return ResiliencePolicies.GetRetryPolicy(logger);
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TImplementation>>();
                return ResiliencePolicies.GetCircuitBreakerPolicy(logger);
            })
            .AddPolicyHandler(ResiliencePolicies.GetTimeoutPolicy())
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
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(name);
                return ResiliencePolicies.GetRetryPolicy(logger);
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(name);
                return ResiliencePolicies.GetCircuitBreakerPolicy(logger);
            })
            .AddPolicyHandler(ResiliencePolicies.GetTimeoutPolicy());
    }
}

/// <summary>
/// Example interface for external service clients
/// </summary>
public interface IExternalPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}

/// <summary>
/// Example implementation with HttpClient
/// </summary>
public class StripePaymentGateway : IExternalPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(HttpClient httpClient, ILogger<StripePaymentGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // HttpClient already has retry + circuit breaker + timeout
        // If external service fails, it will retry automatically
        var response = await _httpClient.PostAsJsonAsync("/v1/charges", request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Payment failed with status {Status}", response.StatusCode);
            return new PaymentResult { Success = false };
        }

        return new PaymentResult { Success = true };
    }
}

public class PaymentRequest { }
public class PaymentResult { public bool Success { get; set; } }

