using LoyaltyService.Domain.Entities;
using SharedKernel.Models;

namespace LoyaltyService.Application.Interfaces;

public interface ILoyaltyMemberRepository : IRepository<LoyaltyMember>
{
    Task<LoyaltyMember?> GetByUserIdAsync(Guid userId);
    Task<PaginatedResult<LoyaltyMember>> GetMembersByProgramAsync(Guid programId, int page, int pageSize);
    Task<IEnumerable<LoyaltyMember>> GetTopMembersByPointsAsync(int count);
}
