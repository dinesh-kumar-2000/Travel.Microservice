using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SharedKernel.RateLimiting;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next, 
        IMemoryCache cache, 
        ILogger<RateLimitingMiddleware> logger,
        RateLimitingOptions options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var key = $"rate_limit_{clientId}";

        var requestCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.WindowInMinutes);
            return 0;
        });

        if (requestCount >= _options.MaxRequestsPerWindow)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Requests: {RequestCount}/{MaxRequests}", 
                clientId, requestCount, _options.MaxRequestsPerWindow);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", (_options.WindowInMinutes * 60).ToString());
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        _cache.Set(key, requestCount + 1);
        
        context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequestsPerWindow.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (_options.MaxRequestsPerWindow - requestCount - 1).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(_options.WindowInMinutes).ToUnixTimeSeconds().ToString());

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get client IP from various headers
        var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                      context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                      context.Connection.RemoteIpAddress?.ToString() ??
                      "unknown";

        return clientIp;
    }
}

public class RateLimitingOptions
{
    public int MaxRequestsPerWindow { get; set; } = 100;
    public int WindowInMinutes { get; set; } = 1;
}
