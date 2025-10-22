using Microsoft.Extensions.DependencyInjection;
using BookingService.Domain.Repositories;
using BookingService.Infrastructure.Repositories;
using SharedKernel.Data;
using System.Reflection;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext from SharedKernel
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Register repositories
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();

        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        // Use centralized DatabaseInitializer from SharedKernel
        DatabaseInitializer.Initialize(connectionString, Assembly.GetExecutingAssembly());
    }
}
