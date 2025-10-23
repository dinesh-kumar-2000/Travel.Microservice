using MediatR;
using IdentityService.Application.DTOs.Responses.Role;

namespace IdentityService.Application.Queries.Role;

public class GetAllRolesQuery : IRequest<IEnumerable<RoleResponse>>
{
}
