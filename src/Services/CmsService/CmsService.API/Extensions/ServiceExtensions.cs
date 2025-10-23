using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CmsService.API.Extensions;

/// <summary>
/// Extension methods for configuring the CmsService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add CmsService API services
    /// </summary>
    public static IServiceCollection AddCmsServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("CmsServicePolicy", policy =>
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
/// Extension methods for configuring the CmsService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure CmsService API pipeline
    /// </summary>
    public static IApplicationBuilder UseCmsServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("CmsServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.CmsStatusHub>("/cmsservice-hub");
        });

        return app;
    }
}
