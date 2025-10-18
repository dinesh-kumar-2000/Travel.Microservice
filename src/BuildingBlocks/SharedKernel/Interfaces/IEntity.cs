namespace SharedKernel.Interfaces;

public interface IEntity<TId>
{
    TId Id { get; }
}

