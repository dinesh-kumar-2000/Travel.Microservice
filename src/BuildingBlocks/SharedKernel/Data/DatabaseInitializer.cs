using DbUp;
using System.Reflection;

namespace SharedKernel.Data;

/// <summary>
/// Centralized database initialization using DbUp
/// Eliminates duplicate DatabaseInitializer implementations across services
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes a PostgreSQL database with embedded migration scripts
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="assembly">Assembly containing embedded migration scripts</param>
    public static void Initialize(string connectionString, Assembly assembly)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(assembly)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception($"Database migration failed: {result.Error.Message}", result.Error);
        }
    }

    /// <summary>
    /// Initializes a PostgreSQL database with embedded migration scripts from calling assembly
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public static void InitializeFromCallingAssembly(string connectionString)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        Initialize(connectionString, callingAssembly);
    }
}

