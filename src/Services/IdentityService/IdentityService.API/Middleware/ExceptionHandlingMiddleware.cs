using System.Net;
using System.Text.Json;
using SharedKernel.Exceptions;

namespace IdentityService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                validationEx.Message,
                (object?)validationEx.Errors
            ),
            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                (object?)null
            ),
            UnauthorizedException unauthorizedEx => (
                (int)HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                (object?)null
            ),
            ForbiddenException forbiddenEx => (
                (int)HttpStatusCode.Forbidden,
                forbiddenEx.Message,
                (object?)null
            ),
            DomainException domainEx => (
                (int)HttpStatusCode.BadRequest,
                domainEx.Message,
                (object?)null
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An internal server error occurred",
                (object?)null
            )
        };

        var result = new
        {
            status = statusCode,
            message = message,
            errors = errors
        };

        response.StatusCode = statusCode;
        await response.WriteAsync(JsonSerializer.Serialize(result));
    }
}

