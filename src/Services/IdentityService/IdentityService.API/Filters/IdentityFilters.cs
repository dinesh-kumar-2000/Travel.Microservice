using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityService.API.Filters;

/// <summary>
/// Custom action filter for identityservice operations
/// </summary>
public class IdentityActionFilter : IActionFilter
{
    private readonly ILogger<IdentityActionFilter> _logger;

    public IdentityActionFilter(ILogger<IdentityActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing identityservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed identityservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for identityservice operations
/// </summary>
public class IdentityExceptionFilter : IExceptionFilter
{
    private readonly ILogger<IdentityExceptionFilter> _logger;

    public IdentityExceptionFilter(ILogger<IdentityExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in identityservice service");

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
