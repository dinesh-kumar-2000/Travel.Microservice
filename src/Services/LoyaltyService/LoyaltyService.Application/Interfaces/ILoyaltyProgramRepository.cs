using LoyaltyService.Domain.Entities;
using SharedKernel.Models;

namespace LoyaltyService.Application.Interfaces;

public interface ILoyaltyProgramRepository : IRepository<LoyaltyProgram>
{
    Task<IEnumerable<LoyaltyProgram>> GetActiveProgramsAsync();
    Task<LoyaltyProgram?> GetByTierNameAsync(string tierName);
}
