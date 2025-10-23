using FlightService.Domain.Entities;
using SharedKernel.Data;

namespace FlightService.Application.Interfaces;

public interface IAirlineRepository : IBaseRepository<Airline, string>
{
    Task<Airline?> GetByCodeAsync(string code);
    Task<IEnumerable<Airline>> GetActiveAirlinesAsync();
}
