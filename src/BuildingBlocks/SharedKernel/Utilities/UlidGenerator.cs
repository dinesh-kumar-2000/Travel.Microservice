namespace SharedKernel.Utilities;

public static class UlidGenerator
{
    public static string Generate() => Ulid.NewUlid().ToString();
    
    public static bool TryParse(string value, out Ulid ulid)
    {
        return Ulid.TryParse(value, out ulid);
    }
}

