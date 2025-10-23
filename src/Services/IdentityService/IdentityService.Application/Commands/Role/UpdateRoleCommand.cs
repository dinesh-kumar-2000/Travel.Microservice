using MediatR;
using IdentityService.Contracts.Responses.Role;

namespace IdentityService.Application.Commands.Role;

public class UpdateRoleCommand : IRequest<RoleResponse>
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}
