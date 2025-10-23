namespace PaymentService.Domain.ValueObjects;

public class Currency
{
    public string Code { get; private set; }
    public string Symbol { get; private set; }
    public int DecimalPlaces { get; private set; }

    private static readonly Dictionary<string, (string Symbol, int DecimalPlaces)> CurrencyInfo = new()
    {
        { "USD", ("$", 2) },
        { "EUR", ("€", 2) },
        { "GBP", ("£", 2) },
        { "JPY", ("¥", 0) },
        { "CAD", ("C$", 2) },
        { "AUD", ("A$", 2) },
        { "CHF", ("CHF", 2) },
        { "CNY", ("¥", 2) },
        { "INR", ("₹", 2) },
        { "BRL", ("R$", 2) }
    };

    private Currency() 
    { 
        Code = "USD";
        Symbol = "$";
        DecimalPlaces = 2;
    }

    public Currency(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Currency code cannot be null or empty", nameof(code));

        var upperCode = code.ToUpperInvariant();
        if (!CurrencyInfo.ContainsKey(upperCode))
            throw new ArgumentException($"Unsupported currency code: {code}", nameof(code));

        Code = upperCode;
        Symbol = CurrencyInfo[upperCode].Symbol;
        DecimalPlaces = CurrencyInfo[upperCode].DecimalPlaces;
    }

    public static Currency USD => new("USD");
    public static Currency EUR => new("EUR");
    public static Currency GBP => new("GBP");

    public static implicit operator string(Currency currency) => currency.Code;
    public static implicit operator Currency(string code) => new(code);

    public override string ToString() => Code;
    public override bool Equals(object? obj) => obj is Currency currency && Code == currency.Code;
    public override int GetHashCode() => Code.GetHashCode();
}
