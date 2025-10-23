using MediatR;
using IdentityService.Application.DTOs.Responses.Role;

namespace IdentityService.Application.Commands.Role;

public class CreateRoleCommand : IRequest<RoleResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}
