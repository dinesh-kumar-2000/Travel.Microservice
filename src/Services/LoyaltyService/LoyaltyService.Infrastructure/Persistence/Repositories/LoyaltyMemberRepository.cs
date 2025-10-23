using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Models;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyMemberRepository : BaseRepository<LoyaltyMember>, ILoyaltyMemberRepository
{
    public LoyaltyMemberRepository(DapperContext context) : base(context)
    {
    }

    public async Task<LoyaltyMember?> GetByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT * FROM loyalty_members 
            WHERE user_id = @UserId AND is_active = true AND is_deleted = false";
        
        return await QueryFirstOrDefaultAsync<LoyaltyMember>(sql, new { UserId = userId });
    }

    public async Task<PaginatedResult<LoyaltyMember>> GetMembersByProgramAsync(Guid programId, int page, int pageSize)
    {
        const string sql = @"
            SELECT * FROM loyalty_members 
            WHERE program_id = @ProgramId AND is_active = true AND is_deleted = false 
            ORDER BY points_balance DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        const string countSql = "SELECT COUNT(*) FROM loyalty_members WHERE program_id = @ProgramId AND is_active = true AND is_deleted = false";
        
        var offset = (page - 1) * pageSize;
        var items = await QueryAsync<LoyaltyMember>(sql, new { ProgramId = programId, PageSize = pageSize, Offset = offset });
        var totalCount = await QuerySingleAsync<int>(countSql, new { ProgramId = programId });
        
        return new PaginatedResult<LoyaltyMember>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<LoyaltyMember>> GetTopMembersByPointsAsync(int count)
    {
        const string sql = @"
            SELECT * FROM loyalty_members 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY points_balance DESC 
            LIMIT @Count";
        
        return await QueryAsync<LoyaltyMember>(sql, new { Count = count });
    }
}
