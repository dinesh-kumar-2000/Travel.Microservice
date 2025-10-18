using Microsoft.Extensions.DependencyInjection;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Repositories;
using DbUp;
using System.Reflection;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IPackageRepository>(sp => 
        {
            var tenantContext = sp.GetRequiredService<Tenancy.ITenantContext>();
            var cache = sp.GetRequiredService<SharedKernel.Caching.ICacheService>();
            return new PackageRepository(connectionString, tenantContext, cache);
        });
        
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

