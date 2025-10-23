using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotificationService.API.Filters;

/// <summary>
/// Custom action filter for notificationservice operations
/// </summary>
public class NotificationActionFilter : IActionFilter
{
    private readonly ILogger<NotificationActionFilter> _logger;

    public NotificationActionFilter(ILogger<NotificationActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing notificationservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed notificationservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for notificationservice operations
/// </summary>
public class NotificationExceptionFilter : IExceptionFilter
{
    private readonly ILogger<NotificationExceptionFilter> _logger;

    public NotificationExceptionFilter(ILogger<NotificationExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in notificationservice service");

        context.Result = new ObjectResult(new
        {
            error = "An error occurred while processing your request",
            message = context.Exception.Message,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}
