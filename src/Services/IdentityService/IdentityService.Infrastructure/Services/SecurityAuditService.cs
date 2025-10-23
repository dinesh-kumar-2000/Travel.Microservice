using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using SharedKernel.Caching;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Security audit service implementation
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly SecurityAuditSettings _settings;
    private readonly ICacheService _cacheService;

    public SecurityAuditService(
        ILogger<SecurityAuditService> logger,
        IOptions<SecurityAuditSettings> settings,
        ICacheService cacheService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task LogSecurityEventAsync(
        SecurityEventType eventType,
        string userId,
        string? tenantId = null,
        string? clientIpAddress = null,
        string? userAgent = null,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEvent = new SecurityAuditEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = eventType,
                UserId = userId,
                TenantId = tenantId,
                Timestamp = DateTime.UtcNow,
                ClientIpAddress = clientIpAddress,
                UserAgent = userAgent,
                AdditionalData = additionalData ?? new Dictionary<string, object>()
            };

            // Log to structured logger
            LogSecurityEvent(auditEvent);

            // Store in cache for real-time monitoring
            if (_settings.EnableRealTimeMonitoring)
            {
                await StoreAuditEventAsync(auditEvent, cancellationToken);
            }

            // Check for suspicious patterns
            await CheckForSuspiciousActivityAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while logging security event for user {UserId}", userId);
        }
    }

    public async Task LogLoginEventAsync(
        string userId,
        string? tenantId,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var additionalData = new Dictionary<string, object>
        {
            ["isSuccess"] = isSuccess
        };

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            additionalData["failureReason"] = failureReason;
        }

        var eventType = isSuccess ? SecurityEventType.LoginSuccess : SecurityEventType.LoginFailure;

        await LogSecurityEventAsync(
            eventType,
            userId,
            tenantId,
            clientIpAddress,
            userAgent,
            additionalData,
            cancellationToken);
    }

    public async Task LogPasswordChangeEventAsync(
        string userId,
        string? tenantId,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var additionalData = new Dictionary<string, object>
        {
            ["isSuccess"] = isSuccess
        };

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            additionalData["failureReason"] = failureReason;
        }

        var eventType = isSuccess ? SecurityEventType.PasswordChangeSuccess : SecurityEventType.PasswordChangeFailure;

        await LogSecurityEventAsync(
            eventType,
            userId,
            tenantId,
            clientIpAddress,
            userAgent,
            additionalData,
            cancellationToken);
    }

    public async Task LogAccountLockoutEventAsync(
        string userId,
        string? tenantId,
        string reason,
        string? clientIpAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var additionalData = new Dictionary<string, object>
        {
            ["reason"] = reason
        };

        await LogSecurityEventAsync(
            SecurityEventType.AccountLocked,
            userId,
            tenantId,
            clientIpAddress,
            userAgent,
            additionalData,
            cancellationToken);
    }

    public async Task LogTwoFactorEventAsync(
        string userId,
        string? tenantId,
        TwoFactorEventType twoFactorEventType,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var additionalData = new Dictionary<string, object>
        {
            ["twoFactorEventType"] = twoFactorEventType.ToString(),
            ["isSuccess"] = isSuccess
        };

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            additionalData["failureReason"] = failureReason;
        }

        var eventType = isSuccess ? SecurityEventType.TwoFactorSuccess : SecurityEventType.TwoFactorFailure;

        await LogSecurityEventAsync(
            eventType,
            userId,
            tenantId,
            clientIpAddress,
            userAgent,
            additionalData,
            cancellationToken);
    }

    public async Task<List<SecurityAuditEvent>> GetSecurityEventsAsync(
        string userId,
        string? tenantId = null,
        SecurityEventType? eventType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query a database
            // For now, we'll return events from cache
            var cacheKey = $"security_events:{userId}";
            var events = await _cacheService.GetAsync<List<SecurityAuditEvent>>(cacheKey) ?? new List<SecurityAuditEvent>();

            var filteredEvents = events.AsQueryable();

            if (!string.IsNullOrEmpty(tenantId))
            {
                filteredEvents = filteredEvents.Where(e => e.TenantId == tenantId);
            }

            if (eventType.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.EventType == eventType.Value);
            }

            if (fromDate.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.Timestamp <= toDate.Value);
            }

            return filteredEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving security events for user {UserId}", userId);
            return new List<SecurityAuditEvent>();
        }
    }

    private void LogSecurityEvent(SecurityAuditEvent auditEvent)
    {
        var logData = new Dictionary<string, object>
        {
            ["EventId"] = auditEvent.Id,
            ["EventType"] = auditEvent.EventType.ToString(),
            ["UserId"] = auditEvent.UserId,
            ["TenantId"] = auditEvent.TenantId ?? "unknown",
            ["Timestamp"] = auditEvent.Timestamp,
            ["ClientIpAddress"] = auditEvent.ClientIpAddress ?? "unknown",
            ["UserAgent"] = auditEvent.UserAgent ?? "unknown"
        };

        // Add additional data
        foreach (var item in auditEvent.AdditionalData)
        {
            logData[item.Key] = item.Value;
        }

        _logger.LogInformation("Security Event: {EventType} for User {UserId} at {Timestamp} from {ClientIp}",
            auditEvent.EventType, auditEvent.UserId, auditEvent.Timestamp, auditEvent.ClientIpAddress);
    }

    private async Task StoreAuditEventAsync(SecurityAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"security_events:{auditEvent.UserId}";
            var events = await _cacheService.GetAsync<List<SecurityAuditEvent>>(cacheKey) ?? new List<SecurityAuditEvent>();
            
            events.Add(auditEvent);
            
            // Keep only the last 1000 events per user
            if (events.Count > 1000)
            {
                events = events.OrderByDescending(e => e.Timestamp).Take(1000).ToList();
            }

            await _cacheService.SetAsync(cacheKey, events, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while storing audit event for user {UserId}", auditEvent.UserId);
        }
    }

    private async Task CheckForSuspiciousActivityAsync(SecurityAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Check for multiple failed login attempts from same IP
            if (auditEvent.EventType == SecurityEventType.LoginFailure)
            {
                var recentFailuresKey = $"recent_login_failures:{auditEvent.ClientIpAddress}";
                var recentFailures = await _cacheService.GetAsync<List<DateTime>>(recentFailuresKey) ?? new List<DateTime>();
                
                var now = DateTime.UtcNow;
                recentFailures.Add(now);
                recentFailures.RemoveAll(t => t < now.AddMinutes(-15)); // Keep only last 15 minutes
                
                await _cacheService.SetAsync(recentFailuresKey, recentFailures, TimeSpan.FromMinutes(15));

                if (recentFailures.Count >= 10) // 10 failed attempts in 15 minutes
                {
                    _logger.LogWarning("Suspicious activity detected: {FailureCount} failed login attempts from IP {ClientIp} in 15 minutes",
                        recentFailures.Count, auditEvent.ClientIpAddress);
                    
                    await LogSecurityEventAsync(
                        SecurityEventType.SuspiciousActivity,
                        auditEvent.UserId,
                        auditEvent.TenantId,
                        auditEvent.ClientIpAddress,
                        auditEvent.UserAgent,
                        new Dictionary<string, object>
                        {
                            ["activityType"] = "MultipleFailedLogins",
                            ["failureCount"] = recentFailures.Count,
                            ["timeWindowMinutes"] = 15
                        },
                        cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking for suspicious activity");
        }
    }
}

/// <summary>
/// Security audit service interface
/// </summary>
public interface ISecurityAuditService
{
    Task LogSecurityEventAsync(
        SecurityEventType eventType,
        string userId,
        string? tenantId = null,
        string? clientIpAddress = null,
        string? userAgent = null,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);

    Task LogLoginEventAsync(
        string userId,
        string? tenantId,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    Task LogPasswordChangeEventAsync(
        string userId,
        string? tenantId,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    Task LogAccountLockoutEventAsync(
        string userId,
        string? tenantId,
        string reason,
        string? clientIpAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task LogTwoFactorEventAsync(
        string userId,
        string? tenantId,
        TwoFactorEventType twoFactorEventType,
        bool isSuccess,
        string? clientIpAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    Task<List<SecurityAuditEvent>> GetSecurityEventsAsync(
        string userId,
        string? tenantId = null,
        SecurityEventType? eventType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Security audit settings
/// </summary>
public class SecurityAuditSettings
{
    public bool EnableRealTimeMonitoring { get; set; } = true;
    public int MaxEventsPerUser { get; set; } = 1000;
    public int EventRetentionDays { get; set; } = 30;
}

/// <summary>
/// Security event types
/// </summary>
public enum SecurityEventType
{
    LoginSuccess,
    LoginFailure,
    Logout,
    PasswordChangeSuccess,
    PasswordChangeFailure,
    PasswordResetRequested,
    PasswordResetCompleted,
    TwoFactorEnabled,
    TwoFactorDisabled,
    TwoFactorSuccess,
    TwoFactorFailure,
    AccountLocked,
    AccountUnlocked,
    SuspiciousActivity,
    DataAccess,
    PermissionChange
}

/// <summary>
/// Two-factor authentication event types
/// </summary>
public enum TwoFactorEventType
{
    Setup,
    Verification,
    BackupCodeUsed,
    RecoveryCodeUsed,
    Disable
}

/// <summary>
/// Security audit event model
/// </summary>
public class SecurityAuditEvent
{
    public string Id { get; set; } = string.Empty;
    public SecurityEventType EventType { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
