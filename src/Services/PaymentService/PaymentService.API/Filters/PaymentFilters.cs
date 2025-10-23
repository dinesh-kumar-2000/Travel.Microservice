using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PaymentService.API.Filters;

/// <summary>
/// Custom action filter for paymentservice operations
/// </summary>
public class PaymentActionFilter : IActionFilter
{
    private readonly ILogger<PaymentActionFilter> _logger;

    public PaymentActionFilter(ILogger<PaymentActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing paymentservice action: {ActionName}", 
            context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed paymentservice action: {ActionName} with result: {Result}", 
            context.ActionDescriptor.DisplayName, 
            context.Result?.GetType().Name ?? "Unknown");
    }
}

/// <summary>
/// Custom exception filter for paymentservice operations
/// </summary>
public class PaymentExceptionFilter : IExceptionFilter
{
    private readonly ILogger<PaymentExceptionFilter> _logger;

    public PaymentExceptionFilter(ILogger<PaymentExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in paymentservice service");

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
