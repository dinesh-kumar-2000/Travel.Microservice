using IdentityService.Application.DTOs.Responses.Role;

namespace IdentityService.Application.DTOs.Responses.Role;

public class RoleListResponse
{
    public List<RoleResponse> Roles { get; set; } = new();
    public int TotalCount { get; set; }
}
