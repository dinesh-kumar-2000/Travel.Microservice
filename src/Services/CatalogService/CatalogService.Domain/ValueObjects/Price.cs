namespace CatalogService.Domain.ValueObjects;

public class Price
{
    public decimal Value { get; private set; }
    public string Currency { get; private set; }

    private Price() 
    { 
        Value = 0;
        Currency = "USD";
    }

    public Price(decimal value, string currency = "USD")
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative", nameof(value));

        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Value = value;
        Currency = currency.ToUpperInvariant();
    }

    public static Price Zero(string currency = "USD") => new(0, currency);

    public static implicit operator decimal(Price price) => price.Value;
    public static implicit operator Price(decimal value) => new(value);

    public override string ToString() => $"{Value:C} {Currency}";
    public override bool Equals(object? obj) => obj is Price price && Value == price.Value && Currency == price.Currency;
    public override int GetHashCode() => HashCode.Combine(Value, Currency);
}
