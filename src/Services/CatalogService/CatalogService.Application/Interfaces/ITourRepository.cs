using CatalogService.Domain.Entities;
using SharedKernel.Interfaces;

namespace CatalogService.Application.Interfaces;

public interface ITourRepository : IRepository<Tour, string>
{
    Task<(IEnumerable<Tour> Tours, int TotalCount)> SearchToursAsync(
        string tenantId,
        string? destination = null,
        int? minDuration = null,
        int? maxDuration = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? difficulty = null,
        DateTime? startDate = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<Tour>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

