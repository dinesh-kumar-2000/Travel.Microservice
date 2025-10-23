using MediatR;
using IdentityService.Contracts.Responses.Role;

namespace IdentityService.Application.Queries.Role;

public class GetRoleQuery : IRequest<RoleResponse?>
{
    public Guid RoleId { get; set; }
}
