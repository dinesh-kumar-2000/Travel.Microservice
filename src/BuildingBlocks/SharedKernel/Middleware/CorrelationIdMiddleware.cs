using Microsoft.AspNetCore.Http;
using SharedKernel.Behaviors;
using SharedKernel.Constants;
using SharedKernel.Utilities;

namespace SharedKernel.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        var correlationId = context.GetOrGenerateCorrelationId();
        correlationIdProvider.CorrelationId = correlationId;
        
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(ApplicationConstants.Headers.CorrelationId, correlationId);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

