using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace CatalogService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        
        // Use centralized behaviors from SharedKernel
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SharedKernel.Behaviors.ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SharedKernel.Behaviors.CorrelationIdBehavior<,>));
        
        return services;
    }
}

