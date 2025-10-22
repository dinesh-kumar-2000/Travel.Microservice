using Dapper;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for user operations including authentication and two-factor auth
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class UserRepository : TenantBaseRepository<User, string>, IUserRepository
{
    protected override string TableName => "users";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public UserRepository(IDapperContext context, ILogger<UserRepository> logger) 
        : base(context, logger)
    {
    }

    #region Overridden Base Methods

    public override async Task<string> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO users (id, tenant_id, email, password_hash, first_name, last_name, 
                phone_number, email_confirmed, phone_number_confirmed, is_active, 
                is_deleted, created_at)
            VALUES (@Id, @TenantId, @Email, @PasswordHash, @FirstName, @LastName, 
                @PhoneNumber, @EmailConfirmed, @PhoneNumberConfirmed, @IsActive, 
                @IsDeleted, @CreatedAt)";

        await connection.ExecuteAsync(sql, entity);
        _logger.LogInformation("User {UserId} created with email {Email}", entity.Id, entity.Email);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET email = @Email,
                password_hash = @PasswordHash,
                first_name = @FirstName,
                last_name = @LastName,
                phone_number = @PhoneNumber,
                email_confirmed = @EmailConfirmed,
                phone_number_confirmed = @PhoneNumberConfirmed,
                is_active = @IsActive,
                last_login_at = @LastLoginAt,
                refresh_token = @RefreshToken,
                refresh_token_expires_at = @RefreshTokenExpiresAt,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("User {UserId} updated", entity.Id);
        }

        return rowsAffected > 0;
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET is_deleted = true, 
                deleted_at = @DeletedAt 
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id,
            DeletedAt = DefaultProviders.DateTimeProvider.UtcNow 
        });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("User {UserId} deleted (soft delete)", id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region User Domain-Specific Methods

    public async Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM users 
            WHERE LOWER(email) = LOWER(@Email) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, TenantId = tenantId });
    }

    public async Task<bool> EmailExistsAsync(string email, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(1) 
            FROM users 
            WHERE LOWER(email) = LOWER(@Email) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, TenantId = tenantId });
        return count > 0;
    }

    #endregion

    #region Role Management Methods

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT r.name 
            FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @UserId";

        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task AssignRoleAsync(string userId, string roleId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(roleId))
            throw new ArgumentNullException(nameof(roleId));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO user_roles (id, user_id, role_id, created_at)
            VALUES (@Id, @UserId, @RoleId, @CreatedAt)
            ON CONFLICT ON CONSTRAINT uq_user_roles DO NOTHING";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = DefaultProviders.IdGenerator.Generate(),
            UserId = userId, 
            RoleId = roleId,
            CreatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        _logger.LogInformation("Role {RoleId} assigned to user {UserId}", roleId, userId);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        using var connection = CreateConnection();
        
        const string sql = "SELECT * FROM fn_get_user_permissions(@UserId)";
        
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    #endregion

    #region Two-Factor Authentication Methods

    public async Task<TwoFactorAuth?> GetTwoFactorAuthAsync(Guid userId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM two_factor_auth
            WHERE user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<TwoFactorAuth>(sql, new { UserId = userId });
    }

    public async Task<bool> SaveTwoFactorSecretAsync(Guid userId, string secret, List<string> backupCodes)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException(nameof(secret));
        if (backupCodes == null || !backupCodes.Any())
            throw new ArgumentException("Backup codes cannot be empty", nameof(backupCodes));

        using var connection = CreateConnection();
        
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

        _logger.LogInformation("Two-factor secret saved for user {UserId}", userId);
        return rowsAffected > 0;
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId, string secret)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException(nameof(secret));

        using var connection = CreateConnection();
        
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
            await LogTwoFactorActivityAsync(userId, "enabled", true);
            _logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        using var connection = CreateConnection();
        
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
            await LogTwoFactorActivityAsync(userId, "disabled", true);
            _logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException(nameof(code));

        using var connection = CreateConnection();
        
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
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        await LogTwoFactorActivityAsync(userId, "backup_used", true);
        _logger.LogInformation("Backup code used successfully for user {UserId}", userId);
        return true;
    }

    public async Task<bool> UpdateBackupCodesAsync(Guid userId, List<string> backupCodes)
    {
        if (backupCodes == null || !backupCodes.Any())
            throw new ArgumentException("Backup codes cannot be empty", nameof(backupCodes));

        using var connection = CreateConnection();
        
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

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Backup codes updated for user {UserId}", userId);
        }

        return rowsAffected > 0;
    }

    public async Task<int> GetBackupCodesRemainingAsync(Guid userId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COALESCE(array_length(backup_codes, 1), 0)
            FROM two_factor_auth
            WHERE user_id = @UserId AND enabled = true";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<bool> UpdateTwoFactorLastUsedAsync(Guid userId)
    {
        using var connection = CreateConnection();
        
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

    public async Task LogTwoFactorActivityAsync(
        Guid userId,
        string action,
        bool success,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrEmpty(action))
            throw new ArgumentNullException(nameof(action));

        using var connection = CreateConnection();
        
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
            CreatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });
    }

    public async Task<bool> VerifyPasswordAsync(Guid userId, string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT password_hash 
            FROM users 
            WHERE id = @UserId AND is_deleted = false";

        var hashedPassword = await connection.QueryFirstOrDefaultAsync<string>(sql, new { UserId = userId });

        if (string.IsNullOrEmpty(hashedPassword))
        {
            _logger.LogWarning("Password verification failed - user {UserId} not found", userId);
            return false;
        }

        // Use BCrypt for password verification
        var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        
        if (!isValid)
        {
            _logger.LogWarning("Password verification failed for user {UserId}", userId);
        }

        return isValid;
    }

    #endregion
}
