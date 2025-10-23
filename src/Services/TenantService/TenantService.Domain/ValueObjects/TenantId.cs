namespace TenantService.Domain.ValueObjects;

public class TenantId
{
    public Guid Value { get; private set; }

    private TenantId() { Value = Guid.Empty; }

    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(value));

        Value = value;
    }

    public static TenantId New() => new(Guid.NewGuid());

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    public static implicit operator TenantId(Guid tenantId) => new(tenantId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is TenantId tenantId && Value == tenantId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
