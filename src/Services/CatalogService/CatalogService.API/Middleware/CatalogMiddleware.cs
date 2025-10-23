using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CatalogService.API.Middleware;

/// <summary>
/// Middleware for logging catalog requests
/// </summary>
public class CatalogLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CatalogLoggingMiddleware> _logger;

    public CatalogLoggingMiddleware(RequestDelegate next, ILogger<CatalogLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Catalog request: {Method} {Path} from {RemoteIpAddress}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        await _next(context);

        _logger.LogInformation("Catalog response: {StatusCode} for {Method} {Path}",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path);
    }
}

/// <summary>
/// Middleware for handling catalog-specific errors
/// </summary>
public class CatalogErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CatalogErrorHandlingMiddleware> _logger;

    public CatalogErrorHandlingMiddleware(RequestDelegate next, ILogger<CatalogErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in catalog middleware");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Internal server error",
                message = "An error occurred while processing your catalog request",
                timestamp = DateTime.UtcNow
            }));
        }
    }
}
