using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SharedKernel.SignalR;

public class SignalRMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SignalRMiddleware> _logger;

    public SignalRMiddleware(RequestDelegate next, ILogger<SignalRMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/hub"))
        {
            _logger.LogInformation("SignalR connection attempt from {RemoteIpAddress}", 
                context.Connection.RemoteIpAddress);
            
            // Add custom headers for SignalR connections
            context.Response.Headers.Add("X-SignalR-Connection", "true");
        }

        await _next(context);
    }
}
