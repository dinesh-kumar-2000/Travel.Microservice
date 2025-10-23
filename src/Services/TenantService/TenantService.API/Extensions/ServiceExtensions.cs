using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TenantService.API.Extensions;

/// <summary>
/// Extension methods for configuring the TenantService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add TenantService API services
    /// </summary>
    public static IServiceCollection AddTenantServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("TenantServicePolicy", policy =>
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
/// Extension methods for configuring the TenantService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure TenantService API pipeline
    /// </summary>
    public static IApplicationBuilder UseTenantServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("TenantServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.TenantStatusHub>("/tenantservice-hub");
        });

        return app;
    }
}
