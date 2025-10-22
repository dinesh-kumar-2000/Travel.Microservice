using Microsoft.Extensions.Logging;
using SharedKernel.Utilities;

namespace SharedKernel.Auditing;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public class AuditEntry
{
    public string Id { get; set; } = DefaultProviders.IdGenerator.Generate();
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DefaultProviders.DateTimeProvider.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        // In production, save to database
        _logger.LogInformation(
            "Audit: {Action} on {EntityType} {EntityId} by {UserId} in tenant {TenantId}",
            entry.Action, entry.EntityType, entry.EntityId, entry.UserId, entry.TenantId);
        
        return Task.CompletedTask;
    }
}

