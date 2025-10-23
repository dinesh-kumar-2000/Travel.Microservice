using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SeoInsuranceService.API.Extensions;

/// <summary>
/// Extension methods for configuring the SeoInsuranceService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add SeoInsuranceService API services
    /// </summary>
    public static IServiceCollection AddSeoInsuranceServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("SeoInsuranceServicePolicy", policy =>
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
/// Extension methods for configuring the SeoInsuranceService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure SeoInsuranceService API pipeline
    /// </summary>
    public static IApplicationBuilder UseSeoInsuranceServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("SeoInsuranceServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.SeoInsuranceStatusHub>("/seoinsuranceservice-hub");
        });

        return app;
    }
}
