using System.Data;

namespace Vicgital.Data.Sql.Abstractions;

/// <summary>
/// Creates ad-hoc, unmanaged connections for services or background jobs that use Dapper without
/// an EF Core <see cref="IUnitOfWork"/> in scope (e.g. a reporting-only worker). Callers own the
/// returned connection and are responsible for opening/disposing it.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
