using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ReviewsService.API.Extensions;

/// <summary>
/// Extension methods for configuring the ReviewsService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add ReviewsService API services
    /// </summary>
    public static IServiceCollection AddReviewsServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("ReviewsServicePolicy", policy =>
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
/// Extension methods for configuring the ReviewsService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure ReviewsService API pipeline
    /// </summary>
    public static IApplicationBuilder UseReviewsServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("ReviewsServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.ReviewsStatusHub>("/reviewsservice-hub");
        });

        return app;
    }
}
