using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.API.Extensions;

/// <summary>
/// Extension methods for configuring the NotificationService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add NotificationService API services
    /// </summary>
    public static IServiceCollection AddNotificationServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("NotificationServicePolicy", policy =>
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
/// Extension methods for configuring the NotificationService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure NotificationService API pipeline
    /// </summary>
    public static IApplicationBuilder UseNotificationServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("NotificationServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.NotificationStatusHub>("/notificationservice-hub");
        });

        return app;
    }
}
