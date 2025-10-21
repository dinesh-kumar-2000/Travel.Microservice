using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace SharedKernel.Data;

/// <summary>
/// Provides a reusable Dapper database context for managing database connections
/// </summary>
public interface IDapperContext
{
    /// <summary>
    /// Creates a new database connection
    /// </summary>
    IDbConnection CreateConnection();
    
    /// <summary>
    /// Creates a new database connection for a specific database
    /// </summary>
    IDbConnection CreateConnection(string databaseName);
}

/// <summary>
/// Implementation of IDapperContext for PostgreSQL databases
/// </summary>
public class DapperContext : IDapperContext
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection string not found");
    }

    public DapperContext(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _configuration = null!;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public IDbConnection CreateConnection(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
        }

        var connectionString = _configuration?.GetConnectionString(databaseName) ?? _connectionString;
        return new NpgsqlConnection(connectionString);
    }
}

