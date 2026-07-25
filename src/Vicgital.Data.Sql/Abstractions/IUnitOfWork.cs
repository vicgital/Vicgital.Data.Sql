using System.Data;

namespace Vicgital.Data.Sql.Abstractions;

/// <summary>
/// Coordinates a single logical unit of work across EF Core change tracking and hand-written
/// Dapper commands, so both commit or roll back together. <see cref="Connection"/> and
/// <see cref="CurrentTransaction"/> are the same underlying ADO.NET objects EF Core is using,
/// which is what lets <see cref="IDapperQueryExecutor"/> participate in an active transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }

    IDbTransaction? CurrentTransaction { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
