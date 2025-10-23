namespace PaymentService.Domain.ValueObjects;

public class PaymentId
{
    public Guid Value { get; private set; }

    private PaymentId() { Value = Guid.Empty; }

    public PaymentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Payment ID cannot be empty", nameof(value));

        Value = value;
    }

    public static PaymentId New() => new(Guid.NewGuid());

    public static implicit operator Guid(PaymentId paymentId) => paymentId.Value;
    public static implicit operator PaymentId(Guid paymentId) => new(paymentId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is PaymentId paymentId && Value == paymentId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
