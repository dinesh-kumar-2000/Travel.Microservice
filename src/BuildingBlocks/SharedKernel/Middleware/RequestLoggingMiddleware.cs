using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace SharedKernel.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context.Request);
        
        _logger.LogInformation("Request started: {Method} {Path} {QueryString} {Body}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.QueryString, 
            requestBody);

        // Store original response body stream
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        stopwatch.Stop();

        var responseBodyContent = await ReadResponseBodyAsync(responseBody);
        
        _logger.LogInformation("Request completed: {Method} {Path} {StatusCode} {ElapsedMs}ms {ResponseBody}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Response.StatusCode, 
            stopwatch.ElapsedMilliseconds,
            responseBodyContent);

        // Copy the response body back to the original stream
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(Stream body)
    {
        body.Position = 0;
        using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
        var responseBody = await reader.ReadToEndAsync();
        body.Position = 0;
        
        return responseBody;
    }
}
