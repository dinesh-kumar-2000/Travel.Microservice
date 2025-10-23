using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Commands.Auth;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}