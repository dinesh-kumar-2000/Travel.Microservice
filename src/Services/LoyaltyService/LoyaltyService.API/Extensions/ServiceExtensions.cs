using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyService.API.Extensions;

/// <summary>
/// Extension methods for configuring the LoyaltyService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add LoyaltyService API services
    /// </summary>
    public static IServiceCollection AddLoyaltyServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("LoyaltyServicePolicy", policy =>
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
/// Extension methods for configuring the LoyaltyService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure LoyaltyService API pipeline
    /// </summary>
    public static IApplicationBuilder UseLoyaltyServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("LoyaltyServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.LoyaltyStatusHub>("/loyaltyservice-hub");
        });

        return app;
    }
}
