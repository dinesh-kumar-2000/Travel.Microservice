using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        DatabaseInitializer.Initialize(connectionString);
    }
}

