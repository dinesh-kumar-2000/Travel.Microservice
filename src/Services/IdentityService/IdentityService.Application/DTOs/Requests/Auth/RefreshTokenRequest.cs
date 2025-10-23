using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.DTOs.Requests.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
