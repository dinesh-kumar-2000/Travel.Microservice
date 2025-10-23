using Dapper;
using TenantService.Domain.Entities;
using TenantService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace TenantService.Infrastructure.Persistence.Repositories;

public class TenantAdminRepository : BaseRepository<TenantAdmin, string>, ITenantAdminRepository
{
    protected override string TableName => "tenant_admins";
    protected override string IdColumnName => "id";

    public TenantAdminRepository(IDapperContext context, ILogger<TenantAdminRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<TenantAdmin>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM tenant_admins 
            WHERE tenant_id = @TenantId 
            AND is_deleted = false";

        return await connection.QueryAsync<TenantAdmin>(sql, new { TenantId = tenantId });
    }

    public async Task<TenantAdmin?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM tenant_admins 
            WHERE user_id = @UserId 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<TenantAdmin>(sql, new { UserId = userId });
    }

    public async Task<bool> IsUserAdminAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(1) 
            FROM tenant_admins 
            WHERE user_id = @UserId 
            AND tenant_id = @TenantId 
            AND is_active = true 
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, TenantId = tenantId });
        return count > 0;
    }

    public async Task<IEnumerable<TenantAdmin>> GetActiveAdminsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM tenant_admins 
            WHERE tenant_id = @TenantId 
            AND is_active = true 
            AND is_deleted = false";

        return await connection.QueryAsync<TenantAdmin>(sql, new { TenantId = tenantId });
    }

    public override async Task<string> AddAsync(TenantAdmin entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO tenant_admins (id, tenant_id, user_id, role, permissions, is_active, assigned_by, assigned_at, created_at, created_by)
            VALUES (@Id, @TenantId, @UserId, @Role, @Permissions, @IsActive, @AssignedBy, @AssignedAt, @CreatedAt, @CreatedBy)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(TenantAdmin entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE tenant_admins 
            SET tenant_id = @TenantId, user_id = @UserId, role = @Role, permissions = @Permissions, 
                is_active = @IsActive, assigned_by = @AssignedBy, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }

}
