using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace SharedKernel.Logging;

/// <summary>
/// Enhanced request logging middleware with correlation ID and performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetCorrelationId(context);
        
        // Add correlation ID to log context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        {
            // Log request start
            LogRequestStart(context, correlationId);
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // Log request completion
                LogRequestCompletion(context, correlationId, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    private void LogRequestStart(HttpContext context, string correlationId)
    {
        var request = context.Request;
        
        _logger.LogInformation(
            "Request started: {Method} {Path} from {RemoteIpAddress}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress?.ToString());
        
        // Log additional request details for debugging
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var headers = GetSafeHeaders(request.Headers);
            var queryString = request.QueryString.ToString();
            
            _logger.LogDebug(
                "Request details: {Headers}, QueryString: {QueryString}",
                headers,
                queryString);
        }
    }

    private void LogRequestCompletion(HttpContext context, string correlationId, long elapsedMs)
    {
        var response = context.Response;
        
        var logLevel = GetLogLevel(response.StatusCode);
        
        _logger.Log(logLevel,
            "Request completed: {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            response.StatusCode,
            elapsedMs);
        
        // Log performance warnings
        if (elapsedMs > 1000) // More than 1 second
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                elapsedMs);
        }
        
        // Log additional response details for debugging
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var responseHeaders = GetSafeHeaders(response.Headers);
            
            _logger.LogDebug(
                "Response details: {Headers}",
                responseHeaders);
        }
    }

    private string GetCorrelationId(HttpContext context)
    {
        // Try to get correlation ID from headers first
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            return correlationId.ToString();
        }
        
        // Fall back to trace identifier
        return context.TraceIdentifier;
    }

    private LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };
    }

    private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var safeHeaders = new Dictionary<string, string>();
        
        foreach (var header in headers)
        {
            // Skip sensitive headers
            if (IsSensitiveHeader(header.Key))
            {
                safeHeaders[header.Key] = "[REDACTED]";
            }
            else
            {
                safeHeaders[header.Key] = string.Join(", ", header.Value);
            }
        }
        
        return safeHeaders;
    }

    private bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "Authorization",
            "Cookie",
            "X-API-Key",
            "X-Auth-Token",
            "X-Forwarded-For",
            "X-Real-IP"
        };
        
        return sensitiveHeaders.Any(sensitive => 
            string.Equals(headerName, sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
