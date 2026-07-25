using System.Data;
using Vicgital.Data.Sql.Abstractions;

namespace Vicgital.Data.Sql.Ado;

/// <summary>
/// ADO.NET-native <see cref="IUnitOfWork"/> for services that use Dapper without EF Core in the
/// mix. Owns one connection for its (typically DI-scoped) lifetime and opens it lazily on first
/// use, so <see cref="IDapperQueryExecutor"/> calls made through it share a transaction when one
/// is active. There's no change tracker here - writes happen immediately via
/// <see cref="IDapperQueryExecutor.ExecuteAsync"/>, so <see cref="SaveChangesAsync"/> is a no-op
/// kept only so callers can share code against <see cref="IUnitOfWork"/> regardless of which
/// implementation is registered.
/// </summary>
public sealed class AdoUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public AdoUnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
    }

    public IDbTransaction? CurrentTransaction => _transaction;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            return Task.CompletedTask;
        }

        _transaction = Connection.BeginTransaction();
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _transaction?.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
