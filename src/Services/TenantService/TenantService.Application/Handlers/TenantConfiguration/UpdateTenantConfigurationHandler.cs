using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Application.Commands.TenantConfiguration;
using TenantService.Application.DTOs.Responses.TenantConfiguration;
using TenantService.Application.Interfaces;
using SharedKernel.Exceptions;

namespace TenantService.Application.Handlers.TenantConfiguration;

public class UpdateTenantConfigurationHandler : IRequestHandler<UpdateTenantConfigurationCommand, TenantConfigurationResponse>
{
    private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
    private readonly ILogger<UpdateTenantConfigurationHandler> _logger;

    public UpdateTenantConfigurationHandler(
        ITenantConfigurationRepository tenantConfigurationRepository,
        ILogger<UpdateTenantConfigurationHandler> logger)
    {
        _tenantConfigurationRepository = tenantConfigurationRepository;
        _logger = logger;
    }

    public async Task<TenantConfigurationResponse> Handle(UpdateTenantConfigurationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating configuration for tenant {TenantId}", request.TenantId);

        var configuration = await _tenantConfigurationRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (configuration == null)
        {
            // Create new configuration if it doesn't exist
            configuration = Domain.Entities.TenantConfiguration.Create(request.TenantId);
        }

        // Update configuration
        configuration.ThemeSettings = request.ThemeSettings ?? configuration.ThemeSettings;
        configuration.FeatureFlags = request.FeatureFlags ?? configuration.FeatureFlags;
        configuration.CustomSettings = request.CustomSettings ?? configuration.CustomSettings;
        configuration.BrandingSettings = request.BrandingSettings ?? configuration.BrandingSettings;
        configuration.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(configuration.Id))
        {
            await _tenantConfigurationRepository.AddAsync(configuration, cancellationToken);
        }
        else
        {
            await _tenantConfigurationRepository.UpdateAsync(configuration, cancellationToken);
        }

        _logger.LogInformation("Configuration updated successfully for tenant {TenantId}", request.TenantId);

        return new TenantConfigurationResponse
        {
            Id = configuration.Id,
            TenantId = configuration.TenantId,
            ThemeSettings = configuration.ThemeSettings,
            FeatureFlags = configuration.FeatureFlags,
            CustomSettings = configuration.CustomSettings,
            BrandingSettings = configuration.BrandingSettings,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt
        };
    }
}
