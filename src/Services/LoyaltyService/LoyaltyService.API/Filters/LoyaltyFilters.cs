using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LoyaltyService.API.Filters;

/// <summary>
/// Custom action filter for loyaltyservice operations
/// </summary>
public class LoyaltyActionFilter : IActionFilter
{
    private readonly ILogger<LoyaltyActionFilter> _logger;

    public LoyaltyActionFilter(ILogger<LoyaltyActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing loyaltyservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed loyaltyservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for loyaltyservice operations
/// </summary>
public class LoyaltyExceptionFilter : IExceptionFilter
{
    private readonly ILogger<LoyaltyExceptionFilter> _logger;

    public LoyaltyExceptionFilter(ILogger<LoyaltyExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in loyaltyservice service");

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
