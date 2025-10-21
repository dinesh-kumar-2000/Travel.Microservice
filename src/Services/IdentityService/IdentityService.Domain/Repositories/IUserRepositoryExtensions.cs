using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public partial interface IUserRepository
{
    // Two-Factor Authentication methods
    Task<TwoFactorAuth?> GetTwoFactorAuthAsync(Guid userId);
    Task<bool> SaveTwoFactorSecretAsync(Guid userId, string secret, List<string> backupCodes);
    Task<bool> EnableTwoFactorAsync(Guid userId, string secret);
    Task<bool> DisableTwoFactorAsync(Guid userId);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string code);
    Task<bool> UpdateBackupCodesAsync(Guid userId, List<string> backupCodes);
    Task<int> GetBackupCodesRemainingAsync(Guid userId);
    Task<bool> UpdateTwoFactorLastUsedAsync(Guid userId);
    Task LogTwoFactorActivityAsync(Guid userId, string action, bool success, string? ipAddress = null, string? userAgent = null);
    Task<bool> VerifyPasswordAsync(Guid userId, string password);
}

