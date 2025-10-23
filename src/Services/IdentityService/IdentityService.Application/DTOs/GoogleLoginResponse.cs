namespace IdentityService.Application.DTOs;

/// <summary>
/// Response model for Google OAuth login
/// </summary>
public class GoogleLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public GoogleUserInfo User { get; set; } = new();
}
