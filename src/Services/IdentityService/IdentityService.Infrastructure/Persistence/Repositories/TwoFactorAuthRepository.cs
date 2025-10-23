using Dapper;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Utilities;
using System.Text.Json;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public interface ITwoFactorAuthRepository
{
    Task<TwoFactorAuth?> GetByUserIdAsync(Guid userId);
    Task<bool> SaveTwoFactorSecretAsync(Guid userId, string secret, List<string> backupCodes);
    Task<bool> EnableTwoFactorAsync(Guid userId, string secret);
    Task<bool> DisableTwoFactorAsync(Guid userId);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string code);
    Task<bool> UpdateBackupCodesAsync(Guid userId, List<string> backupCodes);
    Task<int> GetBackupCodesRemainingAsync(Guid userId);
    Task<bool> UpdateLastUsedAsync(Guid userId);
    Task LogActivityAsync(Guid userId, string action, bool success, string? ipAddress = null, string? userAgent = null);
}

public class TwoFactorAuthRepository : ITwoFactorAuthRepository
{
    private readonly IDapperContext _context;
    private readonly ILogger<TwoFactorAuthRepository> _logger;

    public TwoFactorAuthRepository(
        IDapperContext context,
        ILogger<TwoFactorAuthRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TwoFactorAuth?> GetByUserIdAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   secret AS Secret,
                   backup_codes AS BackupCodes,
                   enabled AS Enabled,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_used_at AS LastUsedAt
            FROM two_factor_auth
            WHERE user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<TwoFactorAuth>(sql, new { UserId = userId });
    }

    public async Task<bool> SaveTwoFactorSecretAsync(Guid userId, string secret, List<string> backupCodes)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            INSERT INTO two_factor_auth (
                id, user_id, secret, backup_codes, enabled, created_at, updated_at
            )
            VALUES (
                @Id, @UserId, @Secret, @BackupCodes, false, @CreatedAt, @UpdatedAt
            )
            ON CONFLICT (user_id) DO UPDATE
            SET secret = @Secret,
                backup_codes = @BackupCodes,
                updated_at = @UpdatedAt";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Secret = secret,
            BackupCodes = backupCodes.ToArray(),
            CreatedAt = DefaultProviders.DateTimeProvider.UtcNow,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId, string secret)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET enabled = true,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId AND secret = @Secret";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Secret = secret,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        if (rowsAffected > 0)
        {
            await LogActivityAsync(userId, "enabled", true);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET enabled = false,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        if (rowsAffected > 0)
        {
            await LogActivityAsync(userId, "disabled", true);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT backup_codes 
            FROM two_factor_auth
            WHERE user_id = @UserId AND enabled = true";

        var backupCodes = await connection.QueryFirstOrDefaultAsync<string[]>(sql, new { UserId = userId });

        if (backupCodes == null || !backupCodes.Contains(code))
        {
            await LogActivityAsync(userId, "backup_used", false);
            return false;
        }

        // Remove used backup code
        var updatedCodes = backupCodes.Where(c => c != code).ToList();

        const string updateSql = @"
            UPDATE two_factor_auth 
            SET backup_codes = @BackupCodes,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        await connection.ExecuteAsync(updateSql, new
        {
            UserId = userId,
            BackupCodes = updatedCodes.ToArray(),
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        await LogActivityAsync(userId, "backup_used", true);
        return true;
    }

    public async Task<bool> UpdateBackupCodesAsync(Guid userId, List<string> backupCodes)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET backup_codes = @BackupCodes,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            BackupCodes = backupCodes.ToArray(),
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<int> GetBackupCodesRemainingAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT COALESCE(array_length(backup_codes, 1), 0)
            FROM two_factor_auth
            WHERE user_id = @UserId AND enabled = true";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<bool> UpdateLastUsedAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET last_used_at = @LastUsedAt,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            LastUsedAt = DefaultProviders.DateTimeProvider.UtcNow,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task LogActivityAsync(
        Guid userId,
        string action,
        bool success,
        string? ipAddress = null,
        string? userAgent = null)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            INSERT INTO two_factor_auth_logs (
                id, user_id, action, ip_address, user_agent, success, created_at
            )
            VALUES (
                @Id, @UserId, @Action, @IpAddress, @UserAgent, @Success, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("2FA activity logged: {Action} for user {UserId}, success: {Success}",
            action, userId, success);
    }
}

