using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlightService.API.Filters;

/// <summary>
/// Custom action filter for flightservice operations
/// </summary>
public class FlightActionFilter : IActionFilter
{
    private readonly ILogger<FlightActionFilter> _logger;

    public FlightActionFilter(ILogger<FlightActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing flightservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed flightservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for flightservice operations
/// </summary>
public class FlightExceptionFilter : IExceptionFilter
{
    private readonly ILogger<FlightExceptionFilter> _logger;

    public FlightExceptionFilter(ILogger<FlightExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in flightservice service");

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
