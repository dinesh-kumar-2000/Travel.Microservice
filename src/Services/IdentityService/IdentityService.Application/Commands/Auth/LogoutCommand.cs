using MediatR;

namespace IdentityService.Application.Commands.Auth;

public class LogoutCommand : IRequest<Unit>
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
