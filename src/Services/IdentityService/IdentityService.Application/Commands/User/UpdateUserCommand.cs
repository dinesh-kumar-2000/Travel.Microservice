using MediatR;
using IdentityService.Contracts.Responses.User;

namespace IdentityService.Application.Commands.User;

public class UpdateUserCommand : IRequest<UserResponse>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
