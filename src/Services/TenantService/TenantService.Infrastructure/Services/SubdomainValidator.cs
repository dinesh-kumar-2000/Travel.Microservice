using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TenantService.Infrastructure.Services;

public class SubdomainValidator
{
    private readonly ILogger<SubdomainValidator> _logger;
    private static readonly string[] ReservedSubdomains = 
    {
        "www", "api", "admin", "app", "mail", "ftp", "blog", "shop", "store", "support",
        "help", "docs", "status", "dev", "test", "staging", "prod", "production"
    };

    public SubdomainValidator(ILogger<SubdomainValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
        {
            _logger.LogWarning("Subdomain validation failed: empty or null");
            return false;
        }

        // Check length
        if (subdomain.Length < 3 || subdomain.Length > 63)
        {
            _logger.LogWarning("Subdomain validation failed: invalid length {Length}", subdomain.Length);
            return false;
        }

        // Check if reserved
        if (ReservedSubdomains.Contains(subdomain.ToLowerInvariant()))
        {
            _logger.LogWarning("Subdomain validation failed: reserved subdomain {Subdomain}", subdomain);
            return false;
        }

        // Check format: alphanumeric and hyphens, not starting/ending with hyphen
        var pattern = @"^[a-zA-Z0-9]([a-zA-Z0-9-]{1,61}[a-zA-Z0-9])?$";
        if (!Regex.IsMatch(subdomain, pattern))
        {
            _logger.LogWarning("Subdomain validation failed: invalid format {Subdomain}", subdomain);
            return false;
        }

        // Check for consecutive hyphens
        if (subdomain.Contains("--"))
        {
            _logger.LogWarning("Subdomain validation failed: consecutive hyphens {Subdomain}", subdomain);
            return false;
        }

        _logger.LogInformation("Subdomain validation passed: {Subdomain}", subdomain);
        return true;
    }

    public string[] GetReservedSubdomains()
    {
        return ReservedSubdomains.ToArray();
    }
}
