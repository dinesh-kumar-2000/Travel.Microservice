using CatalogService.Domain.Entities;
using SharedKernel.Interfaces;

namespace CatalogService.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel, string>
{
    Task<(IEnumerable<Hotel> Hotels, int TotalCount)> SearchHotelsAsync(
        string tenantId,
        string? city = null,
        string? country = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? starRating = null,
        string[]? amenities = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<Hotel>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

