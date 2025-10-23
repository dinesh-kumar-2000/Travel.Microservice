using Microsoft.Extensions.Logging;
using TenantService.Application.Interfaces;

namespace TenantService.Infrastructure.Services;

public class TenantResolver
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantResolver> _logger;

    public TenantResolver(ITenantRepository tenantRepository, ILogger<TenantResolver> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Domain.Entities.Tenant?> ResolveTenantAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;

        try
        {
            // Try to resolve by subdomain first
            var tenant = await _tenantRepository.GetBySubdomainAsync(identifier);
            if (tenant != null)
            {
                _logger.LogInformation("Tenant resolved by subdomain: {Subdomain}", identifier);
                return tenant;
            }

            // Try to resolve by tenant ID
            if (Guid.TryParse(identifier, out var tenantId))
            {
                tenant = await _tenantRepository.GetByIdAsync(tenantId.ToString());
                if (tenant != null)
                {
                    _logger.LogInformation("Tenant resolved by ID: {TenantId}", tenantId);
                    return tenant;
                }
            }

            _logger.LogWarning("Tenant not found for identifier: {Identifier}", identifier);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant for identifier: {Identifier}", identifier);
            return null;
        }
    }

    public async Task<bool> IsValidSubdomainAsync(string subdomain)
    {
        if (string.IsNullOrEmpty(subdomain))
            return false;

        try
        {
            var exists = await _tenantRepository.SubdomainExistsAsync(subdomain);
            return !exists; // Valid if it doesn't exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subdomain validity: {Subdomain}", subdomain);
            return false;
        }
    }
}
