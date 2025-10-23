using CmsService.Application.Commands.Content.CreateContentCommand;
using CmsService.Application.Commands.Content.UpdateContentCommand;
using CmsService.Application.Commands.Content.DeleteContentCommand;
using CmsService.Application.Commands.Page.CreatePageCommand;
using CmsService.Application.Commands.Page.UpdatePageCommand;
using CmsService.Application.Commands.Page.DeletePageCommand;
using CmsService.Application.Commands.Template.CreateTemplateCommand;
using CmsService.Application.Commands.Template.UpdateTemplateCommand;
using CmsService.Application.Commands.Template.DeleteTemplateCommand;
using CmsService.Application.Queries.Content.GetContentQuery;
using CmsService.Application.Queries.Content.GetAllContentQuery;
using CmsService.Application.Queries.Page.GetPageQuery;
using CmsService.Application.Queries.Page.GetAllPagesQuery;
using CmsService.Application.Queries.Template.GetTemplateQuery;
using CmsService.Application.Queries.Template.GetAllTemplatesQuery;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CmsService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Commands
            cfg.RegisterServicesFromAssemblyContaining<CreateContentCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateContentCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteContentCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreatePageCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdatePageCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeletePageCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreateTemplateCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateTemplateCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteTemplateCommand>();
            
            // Queries
            cfg.RegisterServicesFromAssemblyContaining<GetContentQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllContentQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetPageQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllPagesQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetTemplateQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllTemplatesQuery>();
        });

        return services;
    }
}
