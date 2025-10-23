using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CmsService.API.Filters;

/// <summary>
/// Custom action filter for cmsservice operations
/// </summary>
public class CmsActionFilter : IActionFilter
{
    private readonly ILogger<CmsActionFilter> _logger;

    public CmsActionFilter(ILogger<CmsActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing cmsservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed cmsservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for cmsservice operations
/// </summary>
public class CmsExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CmsExceptionFilter> _logger;

    public CmsExceptionFilter(ILogger<CmsExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in cmsservice service");

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
