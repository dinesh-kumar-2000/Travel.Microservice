using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.API.Extensions;

/// <summary>
/// Extension methods for configuring the IdentityService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add IdentityService API services
    /// </summary>
    public static IServiceCollection AddIdentityServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("IdentityServicePolicy", policy =>
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
/// Extension methods for configuring the IdentityService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure IdentityService API pipeline
    /// </summary>
    public static IApplicationBuilder UseIdentityServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("IdentityServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.IdentityStatusHub>("/identityservice-hub");
        });

        return app;
    }
}
