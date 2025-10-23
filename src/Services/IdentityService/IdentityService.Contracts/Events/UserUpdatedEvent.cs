namespace IdentityService.Contracts.Events;

public class UserUpdatedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
