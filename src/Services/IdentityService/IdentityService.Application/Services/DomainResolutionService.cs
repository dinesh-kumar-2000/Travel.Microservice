using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services;

public class DomainResolutionService : IDomainResolutionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DomainResolutionService> _logger;

    public DomainResolutionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DomainResolutionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> ResolveTenantIdFromDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract subdomain from full domain
            var subdomain = ExtractSubdomain(domain);
            
            if (string.IsNullOrEmpty(subdomain))
            {
                _logger.LogWarning("Could not extract subdomain from domain: {Domain}", domain);
                return null;
            }

            // Call TenantService to get tenant ID by subdomain
            var tenantServiceUrl = _configuration["Services:TenantService:Url"] ?? "http://localhost:5002";
            var response = await _httpClient.GetAsync(
                $"{tenantServiceUrl}/api/v1/tenants/by-subdomain/{subdomain}", 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Tenant not found for subdomain: {Subdomain}", subdomain);
                return null;
            }

            var tenant = await response.Content.ReadFromJsonAsync<TenantLookupDto>(cancellationToken: cancellationToken);
            return tenant?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant from domain: {Domain}", domain);
            return null;
        }
    }

    public bool IsMainDomain(string domain)
    {
        var mainDomains = _configuration.GetSection("Authentication:MainDomains").Get<string[]>() 
            ?? new[] { "portal.company.com", "localhost:3000", "localhost" };

        // Normalize domain for comparison
        var normalizedDomain = domain.ToLowerInvariant().Replace("http://", "").Replace("https://", "");
        
        return mainDomains.Any(mainDomain => 
            normalizedDomain.Equals(mainDomain, StringComparison.OrdinalIgnoreCase) ||
            normalizedDomain.StartsWith(mainDomain, StringComparison.OrdinalIgnoreCase));
    }

    public bool ValidateRoleForDomain(string role, string domain)
    {
        var isMain = IsMainDomain(domain);

        return role switch
        {
            "SuperAdmin" => isMain, // SuperAdmin can only login from main domain
            "TenantAdmin" => !isMain, // TenantAdmin can only login from tenant domain
            "Agent" => !isMain, // Agent can only login from tenant domain
            "Customer" => !isMain, // Customer can only login from tenant domain
            _ => false
        };
    }

    private string? ExtractSubdomain(string domain)
    {
        // Remove protocol
        var cleanDomain = domain.ToLowerInvariant()
            .Replace("http://", "")
            .Replace("https://", "")
            .Split(':')[0]; // Remove port if present

        // Split by dots
        var parts = cleanDomain.Split('.');

        // For localhost development (e.g., client1.localhost)
        if (parts.Length == 2 && parts[1] == "localhost")
        {
            return parts[0];
        }

        // For production (e.g., client1.company.com)
        if (parts.Length >= 3)
        {
            return parts[0];
        }

        return null;
    }

    private class TenantLookupDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
    }
}

