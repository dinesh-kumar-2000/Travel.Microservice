namespace IdentityService.Application.Services;

/// <summary>
/// Service for resolving tenant information from domain/subdomain
/// </summary>
public interface IDomainResolutionService
{
    /// <summary>
    /// Resolves tenant ID from domain/subdomain
    /// </summary>
    Task<string?> ResolveTenantIdFromDomainAsync(string domain, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if domain is the main platform domain (for SuperAdmin access)
    /// </summary>
    bool IsMainDomain(string domain);
    
    /// <summary>
    /// Validates if a role is allowed to login from the given domain
    /// </summary>
    bool ValidateRoleForDomain(string role, string domain);
}

