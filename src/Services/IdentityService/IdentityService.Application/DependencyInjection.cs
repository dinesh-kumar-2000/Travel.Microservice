using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Application.Services;
using SharedKernel.Behaviors;
using System.Reflection;

namespace IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Use centralized behaviors from SharedKernel
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationIdBehavior<,>));

        // Application Services
        services.AddHttpClient<IDomainResolutionService, DomainResolutionService>();

        return services;
    }
}

