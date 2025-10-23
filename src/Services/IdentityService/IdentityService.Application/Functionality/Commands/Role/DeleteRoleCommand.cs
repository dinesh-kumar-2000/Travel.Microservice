using MediatR;

namespace IdentityService.Application.Commands.Role;

public class DeleteRoleCommand : IRequest<Unit>
{
    public Guid RoleId { get; set; }
}
