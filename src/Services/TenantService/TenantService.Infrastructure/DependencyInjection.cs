using Microsoft.Extensions.DependencyInjection;
using TenantService.Domain.Repositories;
using TenantService.Infrastructure.Repositories;
using DbUp;
using System.Reflection;

namespace TenantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ITenantRepository>(sp => new TenantRepository(connectionString));
        services.AddScoped<ILandingPageRepository>(sp => new LandingPageRepository(connectionString));
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

