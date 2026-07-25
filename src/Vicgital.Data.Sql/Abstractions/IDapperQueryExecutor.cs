using System.Data;

namespace Vicgital.Data.Sql.Abstractions;

/// <summary>
/// Escape hatch for hand-written SQL (complex reports, joins, bulk reads) that runs against the
/// same connection/transaction as the active <see cref="IUnitOfWork"/>, so it stays consistent
/// with any pending EF Core changes in the same unit of work.
/// </summary>
public interface IDapperQueryExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);
}
