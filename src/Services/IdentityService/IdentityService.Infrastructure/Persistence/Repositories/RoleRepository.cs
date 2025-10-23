using Dapper;
using IdentityService.Domain.Entities;
using IdentityService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class RoleRepository : TenantBaseRepository<Role, string>, IRoleRepository
{
    protected override string TableName => "roles";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public RoleRepository(IDapperContext context, ILogger<RoleRepository> logger) 
        : base(context, logger)
    {
    }

    public override async Task<string> AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();

        const string sql = @"
            INSERT INTO roles (id, name, description, permissions, is_active, created_at, created_by, tenant_id)
            VALUES (@Id, @Name, @Description, @Permissions, @IsActive, @CreatedAt, @CreatedBy, @TenantId)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(Role entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();

        const string sql = @"
            UPDATE roles
            SET name = @Name, description = @Description, permissions = @Permissions,
                is_active = @IsActive, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }

    public async Task<Role?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM roles 
            WHERE LOWER(name) = LOWER(@Name) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name, TenantId = tenantId });
    }

    public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT r.* FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @UserId 
            AND r.tenant_id = @TenantId
            AND r.is_deleted = false";

        return await connection.QueryAsync<Role>(sql, new { UserId = userId.ToString(), TenantId = tenantId });
    }

    public async Task<bool> ExistsByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(1) 
            FROM roles 
            WHERE LOWER(name) = LOWER(@Name) 
            AND tenant_id = @TenantId 
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Name = name, TenantId = tenantId });
        return count > 0;
    }
}
