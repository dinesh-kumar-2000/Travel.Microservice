using Microsoft.Extensions.DependencyInjection;
using TenantService.Domain.Repositories;
using TenantService.Infrastructure.Repositories;
using SharedKernel.Data;
using DbUp;
using System.Reflection;

namespace TenantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext (IMPROVED: Single source of truth for connections)
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Register repositories (IMPROVED: Using IDapperContext)
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILandingPageRepository, LandingPageRepository>();
        services.AddScoped<ISEORepository, SEORepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ICMSRepository, CMSRepository>();
        
        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();
        
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);
    }
}
