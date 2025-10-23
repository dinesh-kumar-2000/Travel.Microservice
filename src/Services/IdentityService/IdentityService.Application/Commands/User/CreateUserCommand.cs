using MediatR;
using IdentityService.Contracts.Responses.User;

namespace IdentityService.Application.Commands.User;

public class CreateUserCommand : IRequest<UserResponse>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
}
