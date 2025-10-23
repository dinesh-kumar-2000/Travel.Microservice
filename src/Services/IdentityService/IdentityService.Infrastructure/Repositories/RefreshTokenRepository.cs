using Dapper;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken, string>, IRefreshTokenRepository
{
    protected override string TableName => "refresh_tokens";
    protected override string IdColumnName => "id";

    public RefreshTokenRepository(IDapperContext context, ILogger<RefreshTokenRepository> logger) 
        : base(context, logger)
    {
    }

    public override async Task<string> AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();

        const string sql = @"
            INSERT INTO refresh_tokens (id, user_id, token, expires_at, is_revoked, created_at, tenant_id)
            VALUES (@Id, @UserId, @Token, @ExpiresAt, @IsRevoked, @CreatedAt, @TenantId)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();

        const string sql = @"
            UPDATE refresh_tokens
            SET user_id = @UserId, token = @Token, expires_at = @ExpiresAt,
                is_revoked = @IsRevoked, revoked_at = @RevokedAt, updated_at = @UpdatedAt
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM refresh_tokens 
            WHERE token = @Token 
            AND is_revoked = false 
            AND expires_at > @Now";

        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new 
        { 
            Token = token, 
            Now = DateTime.UtcNow 
        });
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM refresh_tokens 
            WHERE user_id = @UserId 
            AND is_revoked = false 
            AND expires_at > @Now";

        return await connection.QueryAsync<RefreshToken>(sql, new 
        { 
            UserId = userId, 
            Now = DateTime.UtcNow 
        });
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE refresh_tokens 
            SET is_revoked = true, 
                revoked_at = @RevokedAt 
            WHERE user_id = @UserId 
            AND is_revoked = false";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            UserId = userId, 
            RevokedAt = DateTime.UtcNow 
        });

        _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", rowsAffected, userId);
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            DELETE FROM refresh_tokens 
            WHERE expires_at < @Now 
            OR is_revoked = true";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow });
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", rowsAffected);
        }
    }
}
