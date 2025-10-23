using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SupportService.API.Extensions;

/// <summary>
/// Extension methods for configuring the SupportService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add SupportService API services
    /// </summary>
    public static IServiceCollection AddSupportServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("SupportServicePolicy", policy =>
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
/// Extension methods for configuring the SupportService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure SupportService API pipeline
    /// </summary>
    public static IApplicationBuilder UseSupportServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("SupportServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.SupportStatusHub>("/supportservice-hub");
        });

        return app;
    }
}
