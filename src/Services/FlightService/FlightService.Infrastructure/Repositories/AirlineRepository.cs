using FlightService.Domain.Entities;
using FlightService.Domain.Repositories;
using SharedKernel.Data;

namespace FlightService.Infrastructure.Repositories;

public class AirlineRepository : BaseRepository<Airline>, IAirlineRepository
{
    public AirlineRepository(DapperContext context) : base(context)
    {
    }

    public async Task<Airline?> GetByCodeAsync(string code)
    {
        const string sql = "SELECT * FROM airlines WHERE code = @Code AND is_deleted = false";
        return await QueryFirstOrDefaultAsync<Airline>(sql, new { Code = code });
    }

    public async Task<IEnumerable<Airline>> GetActiveAirlinesAsync()
    {
        const string sql = @"
            SELECT * FROM airlines 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY name";
        
        return await QueryAsync<Airline>(sql);
    }
}
