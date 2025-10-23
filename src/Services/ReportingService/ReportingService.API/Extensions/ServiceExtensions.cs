using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ReportingService.API.Extensions;

/// <summary>
/// Extension methods for configuring the ReportingService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add ReportingService API services
    /// </summary>
    public static IServiceCollection AddReportingServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("ReportingServicePolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}

/// <summary>
/// Extension methods for configuring the ReportingService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure ReportingService API pipeline
    /// </summary>
    public static IApplicationBuilder UseReportingServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("ReportingServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.ReportingStatusHub>("/reportingservice-hub");
        });

        return app;
    }
}
