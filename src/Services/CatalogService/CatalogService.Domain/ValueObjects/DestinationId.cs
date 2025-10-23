namespace CatalogService.Domain.ValueObjects;

public class DestinationId
{
    public Guid Value { get; private set; }

    private DestinationId() { Value = Guid.Empty; }

    public DestinationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Destination ID cannot be empty", nameof(value));

        Value = value;
    }

    public static DestinationId New() => new(Guid.NewGuid());

    public static implicit operator Guid(DestinationId destinationId) => destinationId.Value;
    public static implicit operator DestinationId(Guid destinationId) => new(destinationId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is DestinationId destinationId && Value == destinationId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
