using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.API.Extensions;

/// <summary>
/// Extension methods for configuring the PaymentService API
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add PaymentService API services
    /// </summary>
    public static IServiceCollection AddPaymentServiceApiServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("PaymentServicePolicy", policy =>
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
/// Extension methods for configuring the PaymentService API pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure PaymentService API pipeline
    /// </summary>
    public static IApplicationBuilder UsePaymentServiceApiPipeline(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("PaymentServicePolicy");

        // Use SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<Hubs.PaymentStatusHub>("/paymentservice-hub");
        });

        return app;
    }
}
