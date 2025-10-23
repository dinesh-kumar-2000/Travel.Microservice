using System.Text.RegularExpressions;

namespace IdentityService.Domain.ValueObjects;

public class Password
{
    public string Value { get; private set; }

    private Password() { Value = string.Empty; }

    public Password(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long", nameof(password));

        if (!IsValidPassword(password))
            throw new ArgumentException("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character", nameof(password));

        Value = password;
    }

    private static bool IsValidPassword(string password)
    {
        var hasLower = new Regex(@"[a-z]").IsMatch(password);
        var hasUpper = new Regex(@"[A-Z]").IsMatch(password);
        var hasDigit = new Regex(@"\d").IsMatch(password);
        var hasSpecial = new Regex(@"[@$!%*?&]").IsMatch(password);

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }

    public static implicit operator string(Password password) => password.Value;
    public static implicit operator Password(string password) => new(password);

    public override string ToString() => "***"; // Never expose password in string representation
    public override bool Equals(object? obj) => obj is Password password && Value == password.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
