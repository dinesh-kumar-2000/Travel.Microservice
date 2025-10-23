using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SeoInsuranceService.API.Filters;

/// <summary>
/// Custom action filter for seoinsuranceservice operations
/// </summary>
public class SeoInsuranceActionFilter : IActionFilter
{
    private readonly ILogger<SeoInsuranceActionFilter> _logger;

    public SeoInsuranceActionFilter(ILogger<SeoInsuranceActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing seoinsuranceservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed seoinsuranceservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for seoinsuranceservice operations
/// </summary>
public class SeoInsuranceExceptionFilter : IExceptionFilter
{
    private readonly ILogger<SeoInsuranceExceptionFilter> _logger;

    public SeoInsuranceExceptionFilter(ILogger<SeoInsuranceExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in seoinsuranceservice service");

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
