using IdentityService.Application.DTOs.Responses.Role;
using IdentityService.Application.Commands.Role;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public interface IRoleService
{
    Task<RoleResponse?> GetByIdAsync(Guid id);
    Task<RoleResponse?> GetByNameAsync(string name);
    Task<IEnumerable<RoleResponse>> GetAllAsync();
    Task<RoleResponse> CreateAsync(CreateRoleCommand command);
    Task<RoleResponse> UpdateAsync(UpdateRoleCommand command);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
}
