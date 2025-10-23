namespace IdentityService.Domain.ValueObjects;

public class UserId
{
    public Guid Value { get; private set; }

    private UserId() { Value = Guid.Empty; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(value));

        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static implicit operator Guid(UserId userId) => userId.Value;
    public static implicit operator UserId(Guid userId) => new(userId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is UserId userId && Value == userId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
