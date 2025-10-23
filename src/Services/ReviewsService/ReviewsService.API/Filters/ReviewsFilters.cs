using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReviewsService.API.Filters;

/// <summary>
/// Custom action filter for reviewsservice operations
/// </summary>
public class ReviewsActionFilter : IActionFilter
{
    private readonly ILogger<ReviewsActionFilter> _logger;

    public ReviewsActionFilter(ILogger<ReviewsActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing reviewsservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed reviewsservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for reviewsservice operations
/// </summary>
public class ReviewsExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ReviewsExceptionFilter> _logger;

    public ReviewsExceptionFilter(ILogger<ReviewsExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in reviewsservice service");

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
