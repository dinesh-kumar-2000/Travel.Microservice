using Dapper;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using SharedKernel.Utilities;
using Tenancy;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantContext _tenantContext;

    public UserRepository(IDbConnectionFactory connectionFactory, ITenantContext tenantContext)
    {
        _connectionFactory = connectionFactory;
        _tenantContext = tenantContext;
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id, 
                   tenant_id AS TenantId, 
                   email AS Email, 
                   password_hash AS PasswordHash, 
                   first_name AS FirstName, 
                   last_name AS LastName, 
                   phone_number AS PhoneNumber, 
                   email_confirmed AS EmailConfirmed, 
                   phone_number_confirmed AS PhoneNumberConfirmed, 
                   is_active AS IsActive,
                   last_login_at AS LastLoginAt,
                   refresh_token AS RefreshToken,
                   refresh_token_expires_at AS RefreshTokenExpiresAt,
                   is_deleted AS IsDeleted,
                   deleted_at AS DeletedAt,
                   deleted_by AS DeletedBy,
                   created_at AS CreatedAt,
                   created_by AS CreatedBy,
                   updated_at AS UpdatedAt,
                   updated_by AS UpdatedBy
            FROM users 
            WHERE id = @Id AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id, 
                   tenant_id AS TenantId, 
                   email AS Email, 
                   password_hash AS PasswordHash, 
                   first_name AS FirstName, 
                   last_name AS LastName, 
                   phone_number AS PhoneNumber, 
                   email_confirmed AS EmailConfirmed, 
                   phone_number_confirmed AS PhoneNumberConfirmed, 
                   is_active AS IsActive,
                   last_login_at AS LastLoginAt,
                   refresh_token AS RefreshToken,
                   refresh_token_expires_at AS RefreshTokenExpiresAt,
                   is_deleted AS IsDeleted,
                   deleted_at AS DeletedAt,
                   deleted_by AS DeletedBy,
                   created_at AS CreatedAt,
                   created_by AS CreatedBy,
                   updated_at AS UpdatedAt,
                   updated_by AS UpdatedBy
            FROM users 
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<User>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO users (id, tenant_id, email, password_hash, first_name, last_name, 
                phone_number, email_confirmed, phone_number_confirmed, is_active, 
                is_deleted, created_at)
            VALUES (@Id, @TenantId, @Email, @PasswordHash, @FirstName, @LastName, 
                @PhoneNumber, @EmailConfirmed, @PhoneNumberConfirmed, @IsActive, 
                @IsDeleted, @CreatedAt)";

        await connection.ExecuteAsync(sql, entity);
        return entity.Id;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
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

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET is_deleted = true, 
                deleted_at = @DeletedAt 
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId,
            DeletedAt = DateTime.UtcNow 
        });
    }

    public async Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id, 
                   tenant_id AS TenantId, 
                   email AS Email, 
                   password_hash AS PasswordHash, 
                   first_name AS FirstName, 
                   last_name AS LastName, 
                   phone_number AS PhoneNumber, 
                   email_confirmed AS EmailConfirmed, 
                   phone_number_confirmed AS PhoneNumberConfirmed, 
                   is_active AS IsActive,
                   last_login_at AS LastLoginAt,
                   refresh_token AS RefreshToken,
                   refresh_token_expires_at AS RefreshTokenExpiresAt,
                   is_deleted AS IsDeleted,
                   deleted_at AS DeletedAt,
                   deleted_by AS DeletedBy,
                   created_at AS CreatedAt,
                   created_by AS CreatedBy,
                   updated_at AS UpdatedAt,
                   updated_by AS UpdatedBy
            FROM users 
            WHERE LOWER(email) = LOWER(@Email) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, TenantId = tenantId });
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT r.name 
            FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @UserId";

        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task AssignRoleAsync(string userId, string roleId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO user_roles (id, user_id, role_id, created_at)
            VALUES (@Id, @UserId, @RoleId, @CreatedAt)
            ON CONFLICT ON CONSTRAINT uq_user_roles DO NOTHING";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = UlidGenerator.Generate(),
            UserId = userId, 
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM fn_get_user_permissions(@UserId)";
        
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<bool> EmailExistsAsync(string email, string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT COUNT(1) 
            FROM users 
            WHERE LOWER(email) = LOWER(@Email) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, TenantId = tenantId });
        return count > 0;
    }
}

