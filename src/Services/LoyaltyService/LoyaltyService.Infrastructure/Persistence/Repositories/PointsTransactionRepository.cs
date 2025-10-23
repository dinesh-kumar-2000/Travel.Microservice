using LoyaltyService.Domain.Entities;
using LoyaltyService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Models;

namespace LoyaltyService.Infrastructure.Persistence.Repositories;

public class PointsTransactionRepository : BaseRepository<PointsTransaction>, IPointsTransactionRepository
{
    public PointsTransactionRepository(DapperContext context) : base(context)
    {
    }

    public async Task<PaginatedResult<PointsTransaction>> GetTransactionsByMemberAsync(Guid memberId, int page, int pageSize)
    {
        const string sql = @"
            SELECT * FROM points_transactions 
            WHERE member_id = @MemberId AND is_deleted = false 
            ORDER BY created_at DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        const string countSql = "SELECT COUNT(*) FROM points_transactions WHERE member_id = @MemberId AND is_deleted = false";
        
        var offset = (page - 1) * pageSize;
        var items = await QueryAsync<PointsTransaction>(sql, new { MemberId = memberId, PageSize = pageSize, Offset = offset });
        var totalCount = await QuerySingleAsync<int>(countSql, new { MemberId = memberId });
        
        return new PaginatedResult<PointsTransaction>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PointsTransaction>> GetTransactionsByTypeAsync(string transactionType)
    {
        const string sql = @"
            SELECT * FROM points_transactions 
            WHERE transaction_type = @TransactionType AND is_deleted = false 
            ORDER BY created_at DESC";
        
        return await QueryAsync<PointsTransaction>(sql, new { TransactionType = transactionType });
    }

    public async Task<IEnumerable<PointsTransaction>> GetExpiringTransactionsAsync(DateTime expiryDate)
    {
        const string sql = @"
            SELECT * FROM points_transactions 
            WHERE expiry_date <= @ExpiryDate AND is_deleted = false 
            ORDER BY expiry_date ASC";
        
        return await QueryAsync<PointsTransaction>(sql, new { ExpiryDate = expiryDate });
    }
}
