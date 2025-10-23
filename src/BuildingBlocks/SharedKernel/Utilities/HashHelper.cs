using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Utilities;

public static class HashHelper
{
    public static string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string ComputeSha512Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using var sha512 = SHA512.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha512.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string ComputeMd5Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string ComputeHmacSha256Hash(string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = hmac.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string ComputeHmacSha512Hash(string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = hmac.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string ComputeBcryptHash(string input, int workFactor = 12)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        return BCrypt.Net.BCrypt.HashPassword(input, workFactor);
    }

    public static bool VerifyBcryptHash(string input, string hash)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

        return BCrypt.Net.BCrypt.Verify(input, hash);
    }

    public static string GenerateSalt(int length = 32)
    {
        var saltBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        
        return Convert.ToBase64String(saltBytes);
    }

    public static string ComputeHashWithSalt(string input, string salt)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        if (string.IsNullOrEmpty(salt))
            throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

        var saltedInput = input + salt;
        return ComputeSha256Hash(saltedInput);
    }
}
