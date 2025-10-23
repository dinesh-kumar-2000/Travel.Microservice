using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FlightService.API.Extensions;

/// <summary>
/// Extension methods for configuring the FlightService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add FlightService API services
    /// </summary>
    public static IServiceCollection AddFlightServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("FlightServicePolicy", policy =>
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
/// Extension methods for configuring the FlightService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure FlightService API pipeline
    /// </summary>
    public static IApplicationBuilder UseFlightServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("FlightServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.FlightStatusHub>("/flightservice-hub");
        });

        return app;
    }
}
