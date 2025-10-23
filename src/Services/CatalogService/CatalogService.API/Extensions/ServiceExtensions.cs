using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.API.Extensions;

/// <summary>
/// Extension methods for configuring the Catalog API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Catalog API services
    /// </summary>
    public static IServiceCollection AddCatalogApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("CatalogPolicy", policy =>
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
/// Extension methods for configuring the Catalog API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure Catalog API pipeline
    /// </summary>
    public static IApplicationBuilder UseCatalogApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("CatalogPolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.CatalogStatusHub>("/catalog-hub");
        });

        return app;
    }
}
