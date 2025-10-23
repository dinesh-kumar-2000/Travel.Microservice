using CmsService.Domain.Repositories;
using CmsService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CmsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        
        return services;
    }
}
