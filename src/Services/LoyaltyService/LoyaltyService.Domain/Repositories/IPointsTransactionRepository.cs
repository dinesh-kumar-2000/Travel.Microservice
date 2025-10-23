using LoyaltyService.Domain.Entities;
using SharedKernel.Models;

namespace LoyaltyService.Domain.Repositories;

public interface IPointsTransactionRepository : IRepository<PointsTransaction>
{
    Task<PaginatedResult<PointsTransaction>> GetTransactionsByMemberAsync(Guid memberId, int page, int pageSize);
    Task<IEnumerable<PointsTransaction>> GetTransactionsByTypeAsync(string transactionType);
    Task<IEnumerable<PointsTransaction>> GetExpiringTransactionsAsync(DateTime expiryDate);
}
