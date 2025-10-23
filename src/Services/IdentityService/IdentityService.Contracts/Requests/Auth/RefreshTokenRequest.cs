using System.ComponentModel.DataAnnotations;

namespace IdentityService.Contracts.Requests.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
