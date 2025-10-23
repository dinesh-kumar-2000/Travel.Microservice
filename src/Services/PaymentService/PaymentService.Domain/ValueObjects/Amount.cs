namespace PaymentService.Domain.ValueObjects;

public class Amount
{
    public decimal Value { get; private set; }
    public string Currency { get; private set; }

    private Amount() 
    { 
        Value = 0;
        Currency = "USD";
    }

    public Amount(decimal value, string currency = "USD")
    {
        if (value < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(value));

        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Value = value;
        Currency = currency.ToUpperInvariant();
    }

    public static Amount Zero(string currency = "USD") => new(0, currency);

    public Amount Add(Amount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add amounts with different currencies");

        return new Amount(Value + other.Value, Currency);
    }

    public Amount Subtract(Amount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract amounts with different currencies");

        return new Amount(Value - other.Value, Currency);
    }

    public static implicit operator decimal(Amount amount) => amount.Value;
    public static implicit operator Amount(decimal value) => new(value);

    public override string ToString() => $"{Value:C} {Currency}";
    public override bool Equals(object? obj) => obj is Amount amount && Value == amount.Value && Currency == amount.Currency;
    public override int GetHashCode() => HashCode.Combine(Value, Currency);
}
