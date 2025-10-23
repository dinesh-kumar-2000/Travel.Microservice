namespace IdentityService.Application.DTOs;

/// <summary>
/// Request model for unlocking user account
/// </summary>
public class UnlockAccountRequest
{
    public string Email { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}
