using EventBus.Events;

namespace IdentityService.Contracts.Events;

public class UserRegisteredEvent : IntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

