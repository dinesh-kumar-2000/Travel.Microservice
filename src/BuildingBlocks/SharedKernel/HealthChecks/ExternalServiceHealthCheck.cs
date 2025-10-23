using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Health check for external service dependencies
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly HttpClient _httpClient;

    public ExternalServiceHealthCheck(ILogger<ExternalServiceHealthCheck> logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, object>();
        var issues = new List<string>();
        
        try
        {
            // Check internet connectivity
            var internetCheck = await CheckInternetConnectivityAsync(cancellationToken);
            results["internet_connectivity"] = internetCheck.IsHealthy;
            if (!internetCheck.IsHealthy)
            {
                issues.Add("No internet connectivity");
            }
            
            // Check DNS resolution
            var dnsCheck = await CheckDnsResolutionAsync(cancellationToken);
            results["dns_resolution"] = dnsCheck.IsHealthy;
            if (!dnsCheck.IsHealthy)
            {
                issues.Add("DNS resolution issues");
            }
            
            // Check specific external services if configured
            var externalServices = GetExternalServicesToCheck();
            foreach (var service in externalServices)
            {
                var serviceCheck = await CheckExternalServiceAsync(service, cancellationToken);
                results[$"service_{service.Name}"] = serviceCheck.IsHealthy;
                if (!serviceCheck.IsHealthy)
                {
                    issues.Add($"External service {service.Name} is unavailable");
                }
            }
            
            if (issues.Any())
            {
                return HealthCheckResult.Degraded(
                    $"External service issues: {string.Join(", ", issues)}",
                    data: results);
            }
            
            return HealthCheckResult.Healthy("External services are healthy", results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service health check failed");
            return HealthCheckResult.Unhealthy("External service check failed", ex, results);
        }
    }
    
    private async Task<(bool IsHealthy, string Details)> CheckInternetConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 5000);
            return (reply.Status == IPStatus.Success, $"Ping to 8.8.8.8: {reply.Status}");
        }
        catch (Exception ex)
        {
            return (false, $"Ping failed: {ex.Message}");
        }
    }
    
    private async Task<(bool IsHealthy, string Details)> CheckDnsResolutionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var hostEntry = await System.Net.Dns.GetHostEntryAsync("google.com");
            return (hostEntry.AddressList.Length > 0, $"Resolved {hostEntry.AddressList.Length} addresses");
        }
        catch (Exception ex)
        {
            return (false, $"DNS resolution failed: {ex.Message}");
        }
    }
    
    private async Task<(bool IsHealthy, string Details)> CheckExternalServiceAsync(ExternalService service, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout
            
            var response = await _httpClient.GetAsync(service.Url, cts.Token);
            var isHealthy = response.IsSuccessStatusCode;
            var details = $"Status: {response.StatusCode}, Response time: {response.Headers.Date}";
            
            return (isHealthy, details);
        }
        catch (Exception ex)
        {
            return (false, $"Request failed: {ex.Message}");
        }
    }
    
    private List<ExternalService> GetExternalServicesToCheck()
    {
        // This could be configured via appsettings.json
        return new List<ExternalService>
        {
            new() { Name = "Google", Url = "https://www.google.com" },
            new() { Name = "Cloudflare", Url = "https://www.cloudflare.com" }
        };
    }
}

public class ExternalService
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
