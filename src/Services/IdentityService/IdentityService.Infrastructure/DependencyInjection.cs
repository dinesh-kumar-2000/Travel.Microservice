using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Data;
using System.Reflection;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext from SharedKernel
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();

        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        // Use centralized DatabaseInitializer from SharedKernel
        DatabaseInitializer.Initialize(connectionString, Assembly.GetExecutingAssembly());
    }
}

