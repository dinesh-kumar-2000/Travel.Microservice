using CatalogService.Domain.Entities;
using SharedKernel.Interfaces;
using SharedKernel.Models;

namespace CatalogService.Application.Interfaces;

public interface IDestinationRepository : IRepository<Destination, string>
{
    Task<IEnumerable<Destination>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Destination>> GetByTypeAsync(int destinationType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Destination>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
}
