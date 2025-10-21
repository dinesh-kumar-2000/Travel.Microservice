using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Repositories;
using SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Commands;

public record UpdateTenantCommand(
    string TenantId,
    string Name,
    string ContactEmail,
    string ContactPhone,
    string? CustomDomain
) : IRequest<UpdateTenantResponse>;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, UpdateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        ITenantRepository repository,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UpdateTenantResponse> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating tenant {TenantId}", request.TenantId);

        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new NotFoundException("Tenant not found");
        }

        // Update tenant using reflection (since properties are private set)
        typeof(Domain.Entities.Tenant).GetProperty("Name")!.SetValue(tenant, request.Name);
        typeof(Domain.Entities.Tenant).GetProperty("ContactEmail")!.SetValue(tenant, request.ContactEmail);
        typeof(Domain.Entities.Tenant).GetProperty("ContactPhone")!.SetValue(tenant, request.ContactPhone);
        typeof(Domain.Entities.Tenant).GetProperty("CustomDomain")!.SetValue(tenant, request.CustomDomain);
        typeof(Domain.Entities.Tenant).GetProperty("UpdatedAt")!.SetValue(tenant, DateTime.UtcNow);

        await _repository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Tenant {TenantId} updated successfully", request.TenantId);

        var settings = new TenantSettings(
            Theme: "light",
            PrimaryColor: tenant.Configuration.PrimaryColor,
            SecondaryColor: tenant.Configuration.SecondaryColor,
            LogoUrl: tenant.Configuration.LogoUrl,
            FaviconUrl: null,
            CustomCss: null
        );

        return new UpdateTenantResponse(
            true,
            "Tenant updated successfully",
            new TenantDto(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.ContactEmail,
                tenant.Status.ToString(),
                tenant.Tier.ToString(),
                tenant.Configuration.LogoUrl,
                tenant.Configuration.PrimaryColor,
                tenant.Configuration.SecondaryColor,
                tenant.Status == Domain.Entities.TenantStatus.Active,
                settings
            )
        );
    }
}

