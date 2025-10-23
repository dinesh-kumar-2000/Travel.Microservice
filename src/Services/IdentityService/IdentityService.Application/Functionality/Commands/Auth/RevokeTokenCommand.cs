namespace IdentityService.Application.Commands.Auth;

public class RevokeTokenCommand
{
    public string RefreshToken { get; set; } = string.Empty;
}
