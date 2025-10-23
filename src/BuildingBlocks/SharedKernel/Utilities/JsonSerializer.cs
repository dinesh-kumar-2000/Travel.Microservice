using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedKernel.Utilities;

public static class JsonSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions PrettyPrintOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<T>(T obj, bool prettyPrint = false)
    {
        if (obj == null)
            return string.Empty;

        var options = prettyPrint ? PrettyPrintOptions : DefaultOptions;
        return System.Text.Json.JsonSerializer.Serialize(obj, options);
    }

    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    public static T? Deserialize<T>(string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    public static async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream == null)
            return default;

        try
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON from stream: {ex.Message}", ex);
        }
    }

    public static async Task SerializeAsync<T>(Stream stream, T obj, bool prettyPrint = false, CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (obj == null)
            return;

        var options = prettyPrint ? PrettyPrintOptions : DefaultOptions;
        await System.Text.Json.JsonSerializer.SerializeAsync(stream, obj, options, cancellationToken);
    }

    public static bool TryDeserialize<T>(string json, out T? result)
    {
        result = default;
        
        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string SerializeWithCustomOptions<T>(T obj, JsonSerializerOptions options)
    {
        if (obj == null)
            return string.Empty;

        return System.Text.Json.JsonSerializer.Serialize(obj, options);
    }

    public static JsonElement ParseJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("JSON string cannot be null or empty", nameof(json));

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}", ex);
        }
    }

    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
