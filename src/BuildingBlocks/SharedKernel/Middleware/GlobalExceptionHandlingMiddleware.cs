using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using System.Net;
using System.Text.Json;

namespace SharedKernel.Middleware;

/// <summary>
/// Global exception handling middleware for all services
/// Converts exceptions to proper HTTP responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
        
        _logger.LogError(exception, 
            "An error occurred. CorrelationId: {CorrelationId}, Message: {Message}", 
            correlationId, exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var result = (object)(exception switch
        {
            ValidationException validationEx => new
            {
                status = (int)HttpStatusCode.BadRequest,
                title = "Validation Error",
                message = validationEx.Message,
                errors = validationEx.Errors,
                correlationId
            },
            NotFoundException notFoundEx => new
            {
                status = (int)HttpStatusCode.NotFound,
                title = "Not Found",
                message = notFoundEx.Message,
                errors = (object?)null,
                correlationId
            },
            UnauthorizedException unauthorizedEx => new
            {
                status = (int)HttpStatusCode.Unauthorized,
                title = "Unauthorized",
                message = unauthorizedEx.Message,
                errors = (object?)null,
                correlationId
            },
            ForbiddenException forbiddenEx => new
            {
                status = (int)HttpStatusCode.Forbidden,
                title = "Forbidden",
                message = forbiddenEx.Message,
                errors = (object?)null,
                correlationId
            },
            DomainException domainEx => new
            {
                status = (int)HttpStatusCode.BadRequest,
                title = "Business Rule Violation",
                message = domainEx.Message,
                errors = (object?)null,
                correlationId
            },
            _ => new
            {
                status = (int)HttpStatusCode.InternalServerError,
                title = "Internal Server Error",
                message = "An unexpected error occurred. Please try again later.",
                errors = (object?)null,
                correlationId
            }
        });

        response.StatusCode = (int)result.GetType().GetProperty("status")!.GetValue(result)!;
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await response.WriteAsync(JsonSerializer.Serialize(result, options));
    }
}

