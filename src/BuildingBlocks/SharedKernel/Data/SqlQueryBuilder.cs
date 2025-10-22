using System.Text;

namespace SharedKernel.Data;

/// <summary>
/// Fluent SQL query builder to eliminate SQL string duplication in repositories
/// </summary>
public class SqlQueryBuilder
{
    private readonly string _tableName;
    private readonly List<string> _selectColumns = new();
    private readonly List<string> _whereConditions = new();
    private readonly List<string> _orderByColumns = new();
    private int? _limit;
    private int? _offset;
    private bool _isCount;
    private bool _isExists;

    private SqlQueryBuilder(string tableName)
    {
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    /// <summary>
    /// Creates a new query builder for the specified table
    /// </summary>
    public static SqlQueryBuilder For(string tableName) => new(tableName);

    /// <summary>
    /// Selects specific columns (defaults to *)
    /// </summary>
    public SqlQueryBuilder Select(params string[] columns)
    {
        _selectColumns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// Adds a WHERE condition
    /// </summary>
    public SqlQueryBuilder Where(string columnName, string parameterName = "")
    {
        var param = string.IsNullOrEmpty(parameterName) ? columnName : parameterName;
        _whereConditions.Add($"{columnName} = @{param}");
        return this;
    }

    /// <summary>
    /// Adds a custom WHERE condition
    /// </summary>
    public SqlQueryBuilder WhereCustom(string condition)
    {
        _whereConditions.Add(condition);
        return this;
    }

    /// <summary>
    /// Adds an ORDER BY clause
    /// </summary>
    public SqlQueryBuilder OrderBy(string columnName, bool descending = false)
    {
        _orderByColumns.Add(descending ? $"{columnName} DESC" : columnName);
        return this;
    }

    /// <summary>
    /// Adds LIMIT clause
    /// </summary>
    public SqlQueryBuilder Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    /// <summary>
    /// Adds OFFSET clause
    /// </summary>
    public SqlQueryBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// Builds a COUNT query
    /// </summary>
    public SqlQueryBuilder Count()
    {
        _isCount = true;
        return this;
    }

    /// <summary>
    /// Builds an EXISTS query
    /// </summary>
    public SqlQueryBuilder Exists()
    {
        _isExists = true;
        return this;
    }

    /// <summary>
    /// Builds the final SQL query string
    /// </summary>
    public string Build()
    {
        var sql = new StringBuilder();

        if (_isExists)
        {
            sql.Append("SELECT EXISTS(SELECT 1 FROM ").Append(_tableName);
        }
        else if (_isCount)
        {
            sql.Append("SELECT COUNT(*) FROM ").Append(_tableName);
        }
        else
        {
            var columns = _selectColumns.Count > 0 ? string.Join(", ", _selectColumns) : "*";
            sql.Append("SELECT ").Append(columns).Append(" FROM ").Append(_tableName);
        }

        if (_whereConditions.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", _whereConditions));
        }

        if (_isExists)
        {
            sql.Append(')');
        }

        if (!_isCount && !_isExists && _orderByColumns.Count > 0)
        {
            sql.Append(" ORDER BY ").Append(string.Join(", ", _orderByColumns));
        }

        if (_limit.HasValue)
        {
            sql.Append(" LIMIT @PageSize");
        }

        if (_offset.HasValue)
        {
            sql.Append(" OFFSET @Offset");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Implicit conversion to string for convenience
    /// </summary>
    public static implicit operator string(SqlQueryBuilder builder) => builder.Build();

    /// <summary>
    /// Helper to build a simple SELECT by ID query
    /// </summary>
    public static string SelectById(string tableName, string idColumn)
    {
        return For(tableName).Where(idColumn, "Id").Build();
    }

    /// <summary>
    /// Helper to build a simple SELECT all query
    /// </summary>
    public static string SelectAll(string tableName)
    {
        return For(tableName).Build();
    }

    /// <summary>
    /// Helper to build a DELETE by ID query
    /// </summary>
    public static string DeleteById(string tableName, string idColumn)
    {
        return $"DELETE FROM {tableName} WHERE {idColumn} = @Id";
    }

    /// <summary>
    /// Helper to build a COUNT query
    /// </summary>
    public static string CountAll(string tableName)
    {
        return For(tableName).Count().Build();
    }

    /// <summary>
    /// Helper to build an EXISTS query
    /// </summary>
    public static string ExistsById(string tableName, string idColumn)
    {
        return For(tableName).Where(idColumn, "Id").Exists().Build();
    }

    /// <summary>
    /// Helper to build a paged query
    /// </summary>
    public static string SelectPaged(string tableName, string orderByColumn)
    {
        return For(tableName).OrderBy(orderByColumn).Limit(1).Offset(1).Build();
    }
}

