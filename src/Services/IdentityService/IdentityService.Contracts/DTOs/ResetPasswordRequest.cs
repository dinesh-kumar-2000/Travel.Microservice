namespace IdentityService.Contracts.DTOs;

/// <summary>
/// Request model for resetting password with token
/// </summary>
public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TenantId { get; set; }
}
