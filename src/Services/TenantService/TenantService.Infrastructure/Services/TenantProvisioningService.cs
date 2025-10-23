using Microsoft.Extensions.Logging;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;
using TenantService.Domain.Enums;

namespace TenantService.Infrastructure.Services;

public class TenantProvisioningService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly SubdomainValidator _subdomainValidator;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        ITenantRepository tenantRepository,
        SubdomainValidator subdomainValidator,
        ILogger<TenantProvisioningService> logger)
    {
        _tenantRepository = tenantRepository;
        _subdomainValidator = subdomainValidator;
        _logger = logger;
    }

    public async Task<Tenant> ProvisionTenantAsync(
        string name, 
        string subdomain, 
        string email, 
        string? phoneNumber = null,
        TenantType tenantType = TenantType.Individual)
    {
        _logger.LogInformation("Starting tenant provisioning for {Name} with subdomain {Subdomain}", name, subdomain);

        // Validate subdomain
        if (!_subdomainValidator.IsValidSubdomain(subdomain))
        {
            throw new ArgumentException($"Invalid subdomain: {subdomain}");
        }

        // Check if subdomain is available
        var subdomainExists = await _tenantRepository.SubdomainExistsAsync(subdomain);
        if (subdomainExists)
        {
            throw new InvalidOperationException($"Subdomain {subdomain} is already taken");
        }

        // Create tenant
        var tenant = Tenant.Create(
            Guid.NewGuid().ToString(),
            name,
            subdomain,
            email,
            phoneNumber ?? string.Empty
        );

        await _tenantRepository.AddAsync(tenant);

        _logger.LogInformation("Tenant provisioned successfully: {TenantId} - {Name}", tenant.Id, name);

        return tenant;
    }

    public async Task<bool> ActivateTenantAsync(Guid tenantId)
    {
        _logger.LogInformation("Activating tenant {TenantId}", tenantId);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.ToString());
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for activation: {TenantId}", tenantId);
            return false;
        }

        tenant.Activate();
        tenant.UpdatedAt = DateTime.UtcNow;

        var success = await _tenantRepository.UpdateAsync(tenant);
        
        if (success)
        {
            _logger.LogInformation("Tenant activated successfully: {TenantId}", tenantId);
        }
        else
        {
            _logger.LogError("Failed to activate tenant: {TenantId}", tenantId);
        }

        return success;
    }

    public async Task<bool> SuspendTenantAsync(Guid tenantId, string reason)
    {
        _logger.LogInformation("Suspending tenant {TenantId} - Reason: {Reason}", tenantId, reason);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.ToString());
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for suspension: {TenantId}", tenantId);
            return false;
        }

        tenant.Suspend();
        tenant.UpdatedAt = DateTime.UtcNow;

        var success = await _tenantRepository.UpdateAsync(tenant);
        
        if (success)
        {
            _logger.LogInformation("Tenant suspended successfully: {TenantId}", tenantId);
        }
        else
        {
            _logger.LogError("Failed to suspend tenant: {TenantId}", tenantId);
        }

        return success;
    }
}
