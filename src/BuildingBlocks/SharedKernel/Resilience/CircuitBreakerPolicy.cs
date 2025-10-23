using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace SharedKernel.Resilience;

public static class CircuitBreakerPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> CreateHttpCircuitBreakerPolicy(
        ILogger logger,
        int exceptionsAllowedBeforeBreaking = 5,
        TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(30);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                breakDuration,
                onBreak: (exception, duration) =>
                {
                    logger.LogWarning("Circuit breaker opened for {Duration} due to: {Exception}", 
                        duration, exception.Exception?.Message ?? "Unknown error");
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset - service is healthy again");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open - testing service health");
                });
    }

    public static IAsyncPolicy<T> CreateGenericCircuitBreakerPolicy<T>(
        ILogger logger,
        int exceptionsAllowedBeforeBreaking = 5,
        TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(30);
        return Policy<T>
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                breakDuration,
                onBreak: (exception, duration) =>
                {
                    logger.LogWarning("Circuit breaker opened for {Duration} due to: {Exception}", 
                        duration, exception.Exception?.Message ?? exception.ToString());
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset - operation is healthy again");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open - testing operation health");
                });
    }

    public static IAsyncPolicy CreateActionCircuitBreakerPolicy(
        ILogger logger,
        int exceptionsAllowedBeforeBreaking = 5,
        TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(30);
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                breakDuration,
                onBreak: (exception, duration) =>
                {
                    logger.LogWarning("Circuit breaker opened for {Duration} due to: {Exception}", 
                        duration, exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset - action is healthy again");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open - testing action health");
                });
    }
}
