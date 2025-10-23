using IdentityService.Contracts.Responses.Role;

namespace IdentityService.Contracts.Responses.Role;

public class RoleListResponse
{
    public List<RoleResponse> Roles { get; set; } = new();
    public int TotalCount { get; set; }
}
