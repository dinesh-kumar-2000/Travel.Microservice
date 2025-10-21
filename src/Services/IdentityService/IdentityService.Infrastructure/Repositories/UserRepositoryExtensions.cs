using Dapper;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Repositories;

public partial class UserRepository
{
    public async Task<TwoFactorAuth?> GetTwoFactorAuthAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
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
        using var connection = _connectionFactory.CreateConnection();
        
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId, string secret)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET enabled = true,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId AND secret = @Secret";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Secret = secret,
            UpdatedAt = DateTime.UtcNow
        });

        if (rowsAffected > 0)
        {
            await LogTwoFactorActivityAsync(userId, "enabled", true);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET enabled = false,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        });

        if (rowsAffected > 0)
        {
            await LogTwoFactorActivityAsync(userId, "disabled", true);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT backup_codes 
            FROM two_factor_auth
            WHERE user_id = @UserId AND enabled = true";

        var backupCodes = await connection.QueryFirstOrDefaultAsync<string[]>(sql, new { UserId = userId });

        if (backupCodes == null || !backupCodes.Contains(code))
        {
            await LogTwoFactorActivityAsync(userId, "backup_used", false);
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
            UpdatedAt = DateTime.UtcNow
        });

        await LogTwoFactorActivityAsync(userId, "backup_used", true);
        return true;
    }

    public async Task<bool> UpdateBackupCodesAsync(Guid userId, List<string> backupCodes)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET backup_codes = @BackupCodes,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            BackupCodes = backupCodes.ToArray(),
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<int> GetBackupCodesRemainingAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT COALESCE(array_length(backup_codes, 1), 0)
            FROM two_factor_auth
            WHERE user_id = @UserId AND enabled = true";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<bool> UpdateTwoFactorLastUsedAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE two_factor_auth 
            SET last_used_at = @LastUsedAt,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            LastUsedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task LogTwoFactorActivityAsync(
        Guid userId,
        string action,
        bool success,
        string? ipAddress = null,
        string? userAgent = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
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
    }

    public async Task<bool> VerifyPasswordAsync(Guid userId, string password)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT password_hash 
            FROM users 
            WHERE id = @UserId AND is_deleted = false";

        var hashedPassword = await connection.QueryFirstOrDefaultAsync<string>(sql, new { UserId = userId });

        if (string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        // TODO: Use your password hasher (BCrypt, Identity, etc.)
        // return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        return true; // Placeholder
    }
}

