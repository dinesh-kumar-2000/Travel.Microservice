using System.Security.Cryptography;

namespace SharedKernel.Utilities;

public static class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    private const string AllChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

    public static string GeneratePassword(int length = 12, bool includeUppercase = true, bool includeDigits = true, bool includeSpecialChars = true)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 characters", nameof(length));

        var chars = LowercaseChars;
        
        if (includeUppercase)
            chars += UppercaseChars;
        
        if (includeDigits)
            chars += DigitChars;
        
        if (includeSpecialChars)
            chars += SpecialChars;

        using var rng = RandomNumberGenerator.Create();
        var password = new char[length];
        
        // Ensure at least one character from each required category
        var index = 0;
        password[index++] = GetRandomChar(rng, LowercaseChars);
        
        if (includeUppercase)
            password[index++] = GetRandomChar(rng, UppercaseChars);
        
        if (includeDigits)
            password[index++] = GetRandomChar(rng, DigitChars);
        
        if (includeSpecialChars)
            password[index++] = GetRandomChar(rng, SpecialChars);

        // Fill the rest with random characters
        for (int i = index; i < length; i++)
        {
            password[i] = GetRandomChar(rng, chars);
        }

        // Shuffle the password
        for (int i = 0; i < length; i++)
        {
            var randomIndex = GetRandomInt(rng, i, length);
            (password[i], password[randomIndex]) = (password[randomIndex], password[i]);
        }

        return new string(password);
    }

    public static string GenerateSecurePassword(int length = 16)
    {
        return GeneratePassword(length, true, true, true);
    }

    public static string GenerateSimplePassword(int length = 8)
    {
        return GeneratePassword(length, true, true, false);
    }

    public static string GeneratePin(int length = 6)
    {
        if (length < 4)
            throw new ArgumentException("PIN length must be at least 4 digits", nameof(length));

        using var rng = RandomNumberGenerator.Create();
        var pin = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            pin[i] = GetRandomChar(rng, DigitChars);
        }

        return new string(pin);
    }

    public static string GenerateToken(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var token = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            token[i] = GetRandomChar(rng, AllChars);
        }

        return new string(token);
    }

    public static string GenerateBase64Token(int byteLength = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[byteLength];
        rng.GetBytes(bytes);
        
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public static string GenerateHexToken(int byteLength = 16)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[byteLength];
        rng.GetBytes(bytes);
        
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var index = GetRandomInt(rng, 0, chars.Length);
        return chars[index];
    }

    private static int GetRandomInt(RandomNumberGenerator rng, int min, int max)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return min + (randomValue % (max - min));
    }
}
