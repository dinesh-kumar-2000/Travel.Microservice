using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using SharedKernel.Data;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyProgramRepository : BaseRepository<LoyaltyProgram>, ILoyaltyProgramRepository
{
    public LoyaltyProgramRepository(DapperContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LoyaltyProgram>> GetActiveProgramsAsync()
    {
        const string sql = @"
            SELECT * FROM loyalty_programs 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY minimum_points";
        
        return await QueryAsync<LoyaltyProgram>(sql);
    }

    public async Task<LoyaltyProgram?> GetByTierNameAsync(string tierName)
    {
        const string sql = @"
            SELECT * FROM loyalty_programs 
            WHERE tier_name = @TierName AND is_active = true AND is_deleted = false";
        
        return await QueryFirstOrDefaultAsync<LoyaltyProgram>(sql, new { TierName = tierName });
    }
}
