using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReportingService.API.Filters;

/// <summary>
/// Custom action filter for reportingservice operations
/// </summary>
public class ReportingActionFilter : IActionFilter
{
    private readonly ILogger<ReportingActionFilter> _logger;

    public ReportingActionFilter(ILogger<ReportingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing reportingservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed reportingservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for reportingservice operations
/// </summary>
public class ReportingExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ReportingExceptionFilter> _logger;

    public ReportingExceptionFilter(ILogger<ReportingExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in reportingservice service");

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
