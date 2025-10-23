namespace PaymentService.Domain.ValueObjects;

public class RefundId
{
    public Guid Value { get; private set; }

    private RefundId() { Value = Guid.Empty; }

    public RefundId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Refund ID cannot be empty", nameof(value));

        Value = value;
    }

    public static RefundId New() => new(Guid.NewGuid());

    public static implicit operator Guid(RefundId refundId) => refundId.Value;
    public static implicit operator RefundId(Guid refundId) => new(refundId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is RefundId refundId && Value == refundId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
