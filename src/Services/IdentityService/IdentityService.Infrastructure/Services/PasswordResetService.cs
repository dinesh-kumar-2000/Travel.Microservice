using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Application.Interfaces;
using SharedKernel.Caching;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Password reset service implementation
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly PasswordResetSettings _settings;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IUserRepository userRepository,
        IEmailService emailService,
        ICacheService cacheService,
        IOptions<PasswordResetSettings> settings,
        ILogger<PasswordResetService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PasswordResetResult> InitiatePasswordResetAsync(
        string email,
        string tenantId,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                _logger.LogWarning("Password reset requested for non-existent email {Email} in tenant {TenantId}", email, tenantId);
                return PasswordResetResult.Success(); // Return success to prevent email enumeration
            }

            // Check rate limiting
            var rateLimitKey = $"password_reset_rate_limit:{email}:{tenantId}";
            var rateLimitCountStr = await _cacheService.GetAsync<string>(rateLimitKey);
            var rateLimitCount = int.TryParse(rateLimitCountStr, out var count) ? count : 0;
            
            if (rateLimitCount >= _settings.MaxRequestsPerHour)
            {
                _logger.LogWarning("Password reset rate limit exceeded for {Email} in tenant {TenantId}", email, tenantId);
                return PasswordResetResult.RateLimited();
            }

            // Generate secure reset token
            var resetToken = GenerateSecureToken();
            var tokenHash = HashToken(resetToken);
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.TokenExpiryMinutes);

            // Store reset token in cache
            var cacheKey = $"password_reset_token:{tokenHash}";
            var resetData = new PasswordResetData
            {
                UserId = user.Id,
                Email = email,
                TenantId = tenantId,
                ExpiresAt = expiresAt,
                ClientIpAddress = clientIpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _cacheService.SetAsync(cacheKey, resetData, TimeSpan.FromMinutes(_settings.TokenExpiryMinutes));

            // Update rate limit counter
            await _cacheService.SetAsync(rateLimitKey, (rateLimitCount + 1).ToString(), TimeSpan.FromHours(1));

            // Send password reset email
            var emailSent = await SendPasswordResetEmailAsync(email, user.FirstName, resetToken, cancellationToken);
            
            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to {Email}", email);
                return PasswordResetResult.Failed("Failed to send password reset email");
            }

            _logger.LogInformation("Password reset initiated for user {Email} in tenant {TenantId}", email, tenantId);
            return PasswordResetResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while initiating password reset for {Email}", email);
            return PasswordResetResult.Failed("An error occurred while processing your request");
        }
    }

    public async Task<PasswordResetResult> ValidateResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHash = HashToken(token);
            var cacheKey = $"password_reset_token:{tokenHash}";
            
            var resetData = await _cacheService.GetAsync<PasswordResetData>(cacheKey);
            if (resetData == null)
            {
                _logger.LogWarning("Invalid or expired password reset token used");
                return PasswordResetResult.InvalidToken();
            }

            if (resetData.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired password reset token used for user {Email}", resetData.Email);
                await _cacheService.RemoveAsync(cacheKey);
                return PasswordResetResult.ExpiredToken();
            }

            return PasswordResetResult.ValidToken(resetData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating password reset token");
            return PasswordResetResult.Failed("An error occurred while validating the token");
        }
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate token first
            var validationResult = await ValidateResetTokenAsync(token, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var resetData = validationResult.ResetData!;
            var tokenHash = HashToken(token);
            var cacheKey = $"password_reset_token:{tokenHash}";

            // Get user
            var user = await _userRepository.GetByIdAsync(resetData.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found during password reset", resetData.UserId);
                return PasswordResetResult.Failed("User not found");
            }

            // Update password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatePassword(passwordHash);
            user.UpdateLastPasswordChange();

            var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);
            if (!updateResult)
            {
                _logger.LogError("Failed to update password for user {UserId}", resetData.UserId);
                return PasswordResetResult.Failed("Failed to update password");
            }

            // Remove the reset token
            await _cacheService.RemoveAsync(cacheKey);

            // Log security event
            _logger.LogInformation("Password successfully reset for user {Email} in tenant {TenantId} from IP {ClientIp}",
                resetData.Email, resetData.TenantId, clientIpAddress);

            // Send confirmation email
            await SendPasswordResetConfirmationEmailAsync(resetData.Email, user.FirstName, cancellationToken);

            return PasswordResetResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while resetting password");
            return PasswordResetResult.Failed("An error occurred while resetting your password");
        }
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<bool> SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string resetToken,
        CancellationToken cancellationToken)
    {
        var resetUrl = $"{_settings.ResetUrl}?token={resetToken}";
        
        var templateData = new Dictionary<string, string>
        {
            ["firstName"] = firstName,
            ["resetUrl"] = resetUrl,
            ["expiryMinutes"] = _settings.TokenExpiryMinutes.ToString()
        };

        return await _emailService.SendTemplateEmailAsync(
            email,
            firstName,
            _settings.ResetEmailTemplateId,
            templateData,
            cancellationToken);
    }

    private async Task<bool> SendPasswordResetConfirmationEmailAsync(
        string email,
        string firstName,
        CancellationToken cancellationToken)
    {
        var templateData = new Dictionary<string, string>
        {
            ["firstName"] = firstName,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        return await _emailService.SendTemplateEmailAsync(
            email,
            firstName,
            _settings.ConfirmationEmailTemplateId,
            templateData,
            cancellationToken);
    }
}

/// <summary>
/// Password reset service interface
/// </summary>
public interface IPasswordResetService
{
    Task<PasswordResetResult> InitiatePasswordResetAsync(
        string email,
        string tenantId,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default);

    Task<PasswordResetResult> ValidateResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Password reset settings
/// </summary>
public class PasswordResetSettings
{
    public int TokenExpiryMinutes { get; set; } = 30;
    public int MaxRequestsPerHour { get; set; } = 5;
    public string ResetUrl { get; set; } = string.Empty;
    public string ResetEmailTemplateId { get; set; } = string.Empty;
    public string ConfirmationEmailTemplateId { get; set; } = string.Empty;
}

/// <summary>
/// Password reset data stored in cache
/// </summary>
public class PasswordResetData
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? ClientIpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Password reset result
/// </summary>
public class PasswordResetResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public PasswordResetData? ResetData { get; private set; }
    public bool IsRateLimited { get; private set; }
    public bool IsInvalidToken { get; private set; }
    public bool IsExpiredToken { get; private set; }

    private PasswordResetResult(bool isSuccess, string? errorMessage = null, PasswordResetData? resetData = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ResetData = resetData;
    }

    public static PasswordResetResult Success() => new(true);
    public static PasswordResetResult Failed(string errorMessage) => new(false, errorMessage);
    public static PasswordResetResult RateLimited() => new(false, "Too many password reset requests. Please try again later.") { IsRateLimited = true };
    public static PasswordResetResult InvalidToken() => new(false, "Invalid password reset token.") { IsInvalidToken = true };
    public static PasswordResetResult ExpiredToken() => new(false, "Password reset token has expired.") { IsExpiredToken = true };
    public static PasswordResetResult ValidToken(PasswordResetData resetData) => new(true, null, resetData);
}
