using System.ComponentModel.DataAnnotations;

namespace IdentityService.Contracts.Requests.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
