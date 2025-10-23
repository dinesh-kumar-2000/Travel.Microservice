using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Utilities;

public static class EncryptionHelper
{
    private const int KeySize = 256;
    private const int IvSize = 128;

    public static string Encrypt(string plainText, string password)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
        
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = IvSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Generate key and IV from password
        var key = GenerateKey(password, aes.KeySize / 8);
        var iv = GenerateIV(password, aes.BlockSize / 8);

        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV and encrypted data
        var result = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string cipherText, string password)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));
        
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = IvSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV and encrypted data
            var iv = new byte[aes.BlockSize / 8];
            var encryptedData = new byte[cipherBytes.Length - iv.Length];
            
            Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);

            // Generate key from password
            var key = GenerateKey(password, aes.KeySize / 8);

            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }

    private static byte[] GenerateKey(string password, int keySize)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(passwordBytes);
        
        var key = new byte[keySize];
        Array.Copy(hash, key, Math.Min(hash.Length, keySize));
        
        return key;
    }

    private static byte[] GenerateIV(string password, int ivSize)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + "IV");
        var hash = sha256.ComputeHash(passwordBytes);
        
        var iv = new byte[ivSize];
        Array.Copy(hash, iv, Math.Min(hash.Length, ivSize));
        
        return iv;
    }
}
