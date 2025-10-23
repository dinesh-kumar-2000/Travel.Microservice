using MediatR;
using IdentityService.Contracts.Responses.User;

namespace IdentityService.Application.Queries.User;

public class GetUserByEmailQuery : IRequest<UserResponse?>
{
    public string Email { get; set; } = string.Empty;
}