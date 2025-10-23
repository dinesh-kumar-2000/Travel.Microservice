using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TenantService.API.Filters;

/// <summary>
/// Custom action filter for tenantservice operations
/// </summary>
public class TenantActionFilter : IActionFilter
{
    private readonly ILogger<TenantActionFilter> _logger;

    public TenantActionFilter(ILogger<TenantActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing tenantservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed tenantservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for tenantservice operations
/// </summary>
public class TenantExceptionFilter : IExceptionFilter
{
    private readonly ILogger<TenantExceptionFilter> _logger;

    public TenantExceptionFilter(ILogger<TenantExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in tenantservice service");

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
