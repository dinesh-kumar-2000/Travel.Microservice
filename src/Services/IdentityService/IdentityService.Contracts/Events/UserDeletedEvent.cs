namespace IdentityService.Contracts.Events;

public class UserDeletedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
