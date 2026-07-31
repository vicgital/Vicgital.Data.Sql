using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vicgital.Data.Sql.Abstractions;

namespace Vicgital.Data.Sql.EntityFrameworkCore;

/// <summary>
/// EF Core-backed base repository. Registered as the open-generic implementation of
/// <see cref="IRepository{TEntity,TKey}"/> for simple entities; extend this class when an
/// aggregate needs entity-specific query methods (the protected <see cref="Query"/> property
/// gives derived classes composable LINQ access without widening the shared interface).
/// </summary>
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected DbContext Context { get; }

    protected DbSet<TEntity> DbSet { get; }

    protected IQueryable<TEntity> Query => DbSet.AsQueryable();

    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
        => await DbSet.ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await DbSet.SingleOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(predicate, cancellationToken);

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        => predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await DbSet.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) => DbSet.Update(entity);

    public virtual void Remove(TEntity entity) => DbSet.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);
}
