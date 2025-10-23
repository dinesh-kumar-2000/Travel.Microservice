namespace IdentityService.Contracts.DTOs;

/// <summary>
/// Request model for Google OAuth login
/// </summary>
public class GoogleLoginRequest
{
    public string AuthorizationCode { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}
