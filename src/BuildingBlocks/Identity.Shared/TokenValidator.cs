using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Utilities;

namespace Identity.Shared;

public static class TokenValidator
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        JwtSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IIdGenerator, UlidIdGenerator>();
        services.AddSingleton<IJwtService, JwtService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = TokenValidationParametersFactory.Create(settings);
        });

        return services;
    }
}

