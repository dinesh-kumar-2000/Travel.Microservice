using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SharedKernel.HealthChecks;

/// <summary>
/// Health check for system resources (memory, disk, CPU)
/// </summary>
public class SystemResourcesHealthCheck : IHealthCheck
{
    private readonly ILogger<SystemResourcesHealthCheck> _logger;

    public SystemResourcesHealthCheck(ILogger<SystemResourcesHealthCheck> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var virtualMemory = process.VirtualMemorySize64;
            
            // Get system memory info
            var totalMemory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);
            
            // Check memory usage thresholds
            var memoryThresholdMB = 1024; // 1GB threshold
            var workingSetMB = workingSet / (1024 * 1024);
            var privateMemoryMB = privateMemory / (1024 * 1024);
            
            var issues = new List<string>();
            var data = new Dictionary<string, object>
            {
                ["working_set_mb"] = workingSetMB,
                ["private_memory_mb"] = privateMemoryMB,
                ["virtual_memory_mb"] = virtualMemory / (1024 * 1024),
                ["gc_total_memory_mb"] = totalMemory / (1024 * 1024),
                ["gc_gen0_collections"] = gen0Collections,
                ["gc_gen1_collections"] = gen1Collections,
                ["gc_gen2_collections"] = gen2Collections,
                ["thread_count"] = process.Threads.Count,
                ["handle_count"] = process.HandleCount
            };
            
            // Check memory thresholds
            if (workingSetMB > memoryThresholdMB)
            {
                issues.Add($"High memory usage: {workingSetMB}MB (threshold: {memoryThresholdMB}MB)");
            }
            
            // Check GC pressure
            if (gen2Collections > 10)
            {
                issues.Add($"High GC pressure: {gen2Collections} Gen2 collections");
            }
            
            // Check disk space (if available)
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                foreach (var drive in drives)
                {
                    var freeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    var totalSpaceGB = drive.TotalSize / (1024 * 1024 * 1024);
                    var usagePercent = ((double)(totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;
                    
                    data[$"disk_{drive.Name.Replace(":", "").Replace("\\", "")}_free_gb"] = freeSpaceGB;
                    data[$"disk_{drive.Name.Replace(":", "").Replace("\\", "")}_usage_percent"] = Math.Round(usagePercent, 2);
                    
                    if (usagePercent > 90)
                    {
                        issues.Add($"High disk usage on {drive.Name}: {usagePercent:F1}%");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not check disk space");
            }
            
            if (issues.Any())
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"System resource issues: {string.Join(", ", issues)}",
                    data: data));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy("System resources are healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("System resources check failed", ex));
        }
    }
}
