using IdentityService.Domain.Entities;
using SharedKernel.Interfaces;

namespace IdentityService.Domain.Repositories;

/// <summary>
/// Repository interface for user operations
/// Includes authentication, role management, and two-factor authentication
/// </summary>
public interface IUserRepository : IRepository<User, string>
{
    // User Management
    Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email, string tenantId, CancellationToken cancellationToken = default);
    
    // Role Management
    Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(string userId, string roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    
    // Two-Factor Authentication
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

