using FlightService.Domain.Entities;
using SharedKernel.Data;

namespace FlightService.Domain.Repositories;

public interface IAirlineRepository : IBaseRepository<Airline, string>
{
    Task<Airline?> GetByCodeAsync(string code);
    Task<IEnumerable<Airline>> GetActiveAirlinesAsync();
}
