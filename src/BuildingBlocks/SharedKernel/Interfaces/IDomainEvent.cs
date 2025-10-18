using MediatR;

namespace SharedKernel.Interfaces;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
    string EventId { get; }
}

