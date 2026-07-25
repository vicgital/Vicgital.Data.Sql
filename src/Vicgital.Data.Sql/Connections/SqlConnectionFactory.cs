using System.Data;
using Microsoft.Data.SqlClient;
using Vicgital.Data.Sql.Abstractions;

namespace Vicgital.Data.Sql.Connections;

/// <summary>
/// Default <see cref="IDbConnectionFactory"/> for SQL Server. Used by services that query via
/// Dapper without an EF Core <see cref="Abstractions.IUnitOfWork"/> in scope.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
