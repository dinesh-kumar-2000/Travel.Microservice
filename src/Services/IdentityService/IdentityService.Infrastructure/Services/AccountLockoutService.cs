using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using IdentityService.Domain.Repositories;
using SharedKernel.Caching;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Account lockout service implementation
/// </summary>
public class AccountLockoutService : IAccountLockoutService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly AccountLockoutSettings _settings;
    private readonly ILogger<AccountLockoutService> _logger;
    private readonly ConcurrentDictionary<string, List<FailedLoginAttempt>> _failedAttempts = new();

    public AccountLockoutService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IOptions<AccountLockoutSettings> settings,
        ILogger<AccountLockoutService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginAttemptResult> RecordFailedLoginAsync(
        string email,
        string tenantId,
        string? clientIpAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{email}:{tenantId}";
            var now = DateTime.UtcNow;

            // Get or create failed attempts list
            var failedAttempts = _failedAttempts.GetOrAdd(key, _ => new List<FailedLoginAttempt>());

            // Add new failed attempt
            var attempt = new FailedLoginAttempt
            {
                Timestamp = now,
                ClientIpAddress = clientIpAddress,
                UserAgent = userAgent
            };

            lock (failedAttempts)
            {
                failedAttempts.Add(attempt);
                
                // Remove old attempts outside the time window
                var cutoffTime = now.AddMinutes(-_settings.LockoutTimeWindowMinutes);
                failedAttempts.RemoveAll(a => a.Timestamp < cutoffTime);
            }

            var recentAttempts = failedAttempts.Count(a => a.Timestamp >= now.AddMinutes(-_settings.LockoutTimeWindowMinutes));

            // Check if account should be locked
            if (recentAttempts >= _settings.MaxFailedAttempts)
            {
                await LockAccountAsync(email, tenantId, now.AddMinutes(_settings.LockoutDurationMinutes), cancellationToken);
                
                _logger.LogWarning("Account locked due to {AttemptCount} failed login attempts for {Email} in tenant {TenantId}",
                    recentAttempts, email, tenantId);

                return LoginAttemptResult.AccountLocked(now.AddMinutes(_settings.LockoutDurationMinutes));
            }

            _logger.LogWarning("Failed login attempt {AttemptCount}/{MaxAttempts} for {Email} in tenant {TenantId}",
                recentAttempts, _settings.MaxFailedAttempts, email, tenantId);

            return LoginAttemptResult.Failed(recentAttempts, _settings.MaxFailedAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while recording failed login for {Email}", email);
            return LoginAttemptResult.Failed(0, _settings.MaxFailedAttempts);
        }
    }

    public async Task ClearFailedLoginAttemptsAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{email}:{tenantId}";
            
            // Remove from memory cache
            _failedAttempts.TryRemove(key, out _);

            // Remove from distributed cache
            var cacheKey = $"failed_login_attempts:{key}";
            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Cleared failed login attempts for {Email} in tenant {TenantId}", email, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while clearing failed login attempts for {Email}", email);
        }
    }

    public async Task<bool> IsAccountLockedAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                return true;
            }

            // Check if account is still locked in cache
            var cacheKey = $"account_locked:{email}:{tenantId}";
            var lockData = await _cacheService.GetAsync<AccountLockData>(cacheKey);
            
            if (lockData != null && lockData.LockedUntil > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking if account is locked for {Email}", email);
            return false;
        }
    }

    public async Task<DateTime?> GetLockoutEndTimeAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
            if (user?.LockedUntil.HasValue == true && user.LockedUntil > DateTime.UtcNow)
            {
                return user.LockedUntil;
            }

            // Check cache
            var cacheKey = $"account_locked:{email}:{tenantId}";
            var lockData = await _cacheService.GetAsync<AccountLockData>(cacheKey);
            
            if (lockData != null && lockData.LockedUntil > DateTime.UtcNow)
            {
                return lockData.LockedUntil;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting lockout end time for {Email}", email);
            return null;
        }
    }

    public async Task UnlockAccountAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
            if (user != null)
            {
                user.UnlockAccount();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            // Remove from cache
            var cacheKey = $"account_locked:{email}:{tenantId}";
            await _cacheService.RemoveAsync(cacheKey);

            // Clear failed attempts
            await ClearFailedLoginAttemptsAsync(email, tenantId, cancellationToken);

            _logger.LogInformation("Account unlocked for {Email} in tenant {TenantId}", email, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while unlocking account for {Email}", email);
        }
    }

    private async Task LockAccountAsync(
        string email,
        string tenantId,
        DateTime lockedUntil,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
            if (user != null)
            {
                user.LockAccount(lockedUntil);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            // Store lock data in cache
            var cacheKey = $"account_locked:{email}:{tenantId}";
            var lockData = new AccountLockData
            {
                Email = email,
                TenantId = tenantId,
                LockedAt = DateTime.UtcNow,
                LockedUntil = lockedUntil
            };

            await _cacheService.SetAsync(cacheKey, lockData, lockedUntil - DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while locking account for {Email}", email);
        }
    }
}

/// <summary>
/// Account lockout service interface
/// </summary>
public interface IAccountLockoutService
{
    Task<LoginAttemptResult> RecordFailedLoginAsync(
        string email,
        string tenantId,
        string? clientIpAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task ClearFailedLoginAttemptsAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> IsAccountLockedAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<DateTime?> GetLockoutEndTimeAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task UnlockAccountAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Account lockout settings
/// </summary>
public class AccountLockoutSettings
{
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutTimeWindowMinutes { get; set; } = 15;
    public int LockoutDurationMinutes { get; set; } = 30;
}

/// <summary>
/// Failed login attempt model
/// </summary>
public class FailedLoginAttempt
{
    public DateTime Timestamp { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// Account lock data model
/// </summary>
public class AccountLockData
{
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime LockedAt { get; set; }
    public DateTime LockedUntil { get; set; }
}

/// <summary>
/// Login attempt result
/// </summary>
public class LoginAttemptResult
{
    public bool IsSuccess { get; private set; }
    public bool IsAccountLocked { get; private set; }
    public DateTime? LockoutEndTime { get; private set; }
    public int FailedAttempts { get; private set; }
    public int MaxAttempts { get; private set; }

    private LoginAttemptResult(bool isSuccess, bool isAccountLocked = false, DateTime? lockoutEndTime = null, int failedAttempts = 0, int maxAttempts = 0)
    {
        IsSuccess = isSuccess;
        IsAccountLocked = isAccountLocked;
        LockoutEndTime = lockoutEndTime;
        FailedAttempts = failedAttempts;
        MaxAttempts = maxAttempts;
    }

    public static LoginAttemptResult Success() => new(true);
    public static LoginAttemptResult Failed(int failedAttempts, int maxAttempts) => new(false, false, null, failedAttempts, maxAttempts);
    public static LoginAttemptResult AccountLocked(DateTime lockoutEndTime) => new(false, true, lockoutEndTime);
}
