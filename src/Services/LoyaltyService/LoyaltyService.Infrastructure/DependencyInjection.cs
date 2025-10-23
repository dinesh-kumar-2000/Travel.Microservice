using LoyaltyService.Application.Interfaces;
using LoyaltyService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
        services.AddScoped<ILoyaltyMemberRepository, LoyaltyMemberRepository>();
        services.AddScoped<IPointsTransactionRepository, PointsTransactionRepository>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        
        return services;
    }
}
