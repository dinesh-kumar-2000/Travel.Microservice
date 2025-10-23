namespace IdentityService.Contracts.DTOs;

/// <summary>
/// Request model for initiating password reset
/// </summary>
public class InitiatePasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}
