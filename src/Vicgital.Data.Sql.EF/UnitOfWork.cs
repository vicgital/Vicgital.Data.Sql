using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Vicgital.Data.Sql.Abstractions;

namespace Vicgital.Data.Sql.EntityFrameworkCore;

/// <summary>
/// Wraps a <typeparamref name="TContext"/> so SaveChanges and any Dapper calls made through
/// <see cref="IDapperQueryExecutor"/> against <see cref="Connection"/>/<see cref="CurrentTransaction"/>
/// commit or roll back as one transaction. Does not own the DbContext's lifetime - that stays
/// with the DI container's scope.
/// </summary>
public sealed class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public IDbConnection Connection => _context.Database.GetDbConnection();

    public IDbTransaction? CurrentTransaction => _transaction?.GetDbTransaction();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
