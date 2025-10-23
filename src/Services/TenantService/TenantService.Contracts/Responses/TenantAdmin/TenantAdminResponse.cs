namespace TenantService.Contracts.Responses.TenantAdmin;

public class TenantAdminResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}