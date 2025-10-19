using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Application.Behaviors;
using IdentityService.Application.Services;
using System.Reflection;

namespace IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Application Services
        services.AddHttpClient<IDomainResolutionService, DomainResolutionService>();

        return services;
    }
}

