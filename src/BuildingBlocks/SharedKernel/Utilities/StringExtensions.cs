using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedKernel.Utilities;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNotNullOrEmpty(this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool IsNotNullOrWhiteSpace(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static string ToTitleCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLower());
    }

    public static string ToCamelCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static string ToPascalCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value.Length == 1)
            return value.ToUpperInvariant();

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    public static string ToSnakeCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }

    public static string ToKebabCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
    }

    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (value.IsNullOrEmpty() || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string RemoveWhitespace(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"\s+", "");
    }

    public static string RemoveSpecialCharacters(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"[^a-zA-Z0-9\s]", "");
    }

    public static string RemoveNumbers(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"\d", "");
    }

    public static string RemoveLetters(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Regex.Replace(value, @"[a-zA-Z]", "");
    }

    public static string Reverse(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        var charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static bool IsEmail(this string value)
    {
        if (value.IsNullOrEmpty())
            return false;

        const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(value, emailPattern);
    }

    public static bool IsPhoneNumber(this string value)
    {
        if (value.IsNullOrEmpty())
            return false;

        const string phonePattern = @"^[\+]?[1-9][\d]{0,15}$";
        return Regex.IsMatch(value.RemoveWhitespace(), phonePattern);
    }

    public static bool IsUrl(this string value)
    {
        if (value.IsNullOrEmpty())
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static string MaskEmail(this string email)
    {
        if (email.IsNullOrEmpty() || !email.IsEmail())
            return email;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var username = parts[0];
        var domain = parts[1];

        if (username.Length <= 2)
            return $"*@{domain}";

        var maskedUsername = username[0] + new string('*', username.Length - 2) + username[^1];
        return $"{maskedUsername}@{domain}";
    }

    public static string MaskPhoneNumber(this string phoneNumber)
    {
        if (phoneNumber.IsNullOrEmpty())
            return phoneNumber;

        var cleaned = phoneNumber.RemoveWhitespace();
        if (cleaned.Length < 4)
            return new string('*', cleaned.Length);

        var visibleDigits = Math.Min(4, cleaned.Length);
        var maskedDigits = cleaned.Length - visibleDigits;
        
        return new string('*', maskedDigits) + cleaned.Substring(cleaned.Length - visibleDigits);
    }

    public static string Base64Encode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    public static string Base64Decode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        try
        {
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return value;
        }
    }

    public static string HtmlEncode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return System.Net.WebUtility.HtmlEncode(value);
    }

    public static string HtmlDecode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return System.Net.WebUtility.HtmlDecode(value);
    }

    public static string UrlEncode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Uri.EscapeDataString(value);
    }

    public static string UrlDecode(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return Uri.UnescapeDataString(value);
    }

    public static string[] SplitLines(this string value)
    {
        if (value.IsNullOrEmpty())
            return Array.Empty<string>();

        return value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    public static string Join(this IEnumerable<string> values, string separator = ", ")
    {
        return string.Join(separator, values);
    }

    public static string FormatWith(this string format, params object[] args)
    {
        if (format.IsNullOrEmpty())
            return format;

        return string.Format(format, args);
    }
}
