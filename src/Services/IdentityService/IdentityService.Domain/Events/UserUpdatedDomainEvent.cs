using SharedKernel.Interfaces;

namespace IdentityService.Domain.Events;

public class UserUpdatedDomainEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime OccurredOn { get; }
    public string EventId { get; }

    public UserUpdatedDomainEvent(Guid userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid().ToString();
    }
}
