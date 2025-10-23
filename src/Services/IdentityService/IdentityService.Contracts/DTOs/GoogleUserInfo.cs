namespace IdentityService.Contracts.DTOs;

/// <summary>
/// Google user information model
/// </summary>
public class GoogleUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}
