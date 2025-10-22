using Microsoft.Extensions.DependencyInjection;
using TenantService.Domain.Repositories;
using TenantService.Infrastructure.Repositories;
using SharedKernel.Data;
using System.Reflection;

namespace TenantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext from SharedKernel
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Register repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILandingPageRepository, LandingPageRepository>();
        services.AddScoped<ISEORepository, SEORepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ICMSRepository, CMSRepository>();
        
        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        // Use centralized DatabaseInitializer from SharedKernel
        DatabaseInitializer.Initialize(connectionString, Assembly.GetExecutingAssembly());
    }
}
