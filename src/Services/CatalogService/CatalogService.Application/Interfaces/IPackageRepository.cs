using CatalogService.Domain.Entities;
using SharedKernel.Interfaces;
using SharedKernel.Models;

namespace CatalogService.Application.Interfaces;

public interface IPackageRepository : IRepository<Package, string>
{
    Task<PagedResult<Package>> SearchAsync(string tenantId, string? destination, 
        decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, 
        CancellationToken cancellationToken = default);
    
    Task<bool> ReserveSlotsAsync(string packageId, int quantity, CancellationToken cancellationToken = default);
    
    Task<bool> ReleaseSlotsAsync(string packageId, int quantity, CancellationToken cancellationToken = default);
}

