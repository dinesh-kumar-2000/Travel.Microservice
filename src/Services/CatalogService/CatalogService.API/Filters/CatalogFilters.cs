using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogService.API.Filters;

/// <summary>
/// Custom action filter for catalog operations
/// </summary>
public class CatalogActionFilter : IActionFilter
{
    private readonly ILogger<CatalogActionFilter> _logger;

    public CatalogActionFilter(ILogger<CatalogActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing catalog action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed catalog action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for catalog operations
/// </summary>
public class CatalogExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CatalogExceptionFilter> _logger;

    public CatalogExceptionFilter(ILogger<CatalogExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in catalog service");

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
