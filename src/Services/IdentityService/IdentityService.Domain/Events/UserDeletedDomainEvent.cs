using SharedKernel.Interfaces;

namespace IdentityService.Domain.Events;

public class UserDeletedDomainEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }
    public string EventId { get; }

    public UserDeletedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid().ToString();
    }
}
