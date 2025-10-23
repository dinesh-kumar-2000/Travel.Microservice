using MediatR;
using IdentityService.Contracts.Responses.Role;

namespace IdentityService.Application.Queries.Role;

public class GetAllRolesQuery : IRequest<IEnumerable<RoleResponse>>
{
}
