namespace IdentityService.Contracts.DTOs;

/// <summary>
/// Response model for Google OAuth authorization URL
/// </summary>
public class GoogleAuthUrlResponse
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
