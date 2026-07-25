using System.Data;
using Dapper;
using Vicgital.Data.Sql.Abstractions;

namespace Vicgital.Data.Sql.Dapper;

/// <summary>
/// Runs Dapper commands against the active <see cref="IUnitOfWork"/>'s connection/transaction,
/// so raw SQL reads and writes see the same pending EF Core changes as the rest of the unit of work.
/// </summary>
public sealed class DapperQueryExecutor : IDapperQueryExecutor
{
    private readonly IUnitOfWork _unitOfWork;

    public DapperQueryExecutor(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var command = BuildCommand(sql, parameters, commandType, commandTimeout, cancellationToken);
        var result = await _unitOfWork.Connection.QueryAsync<T>(command);
        return result.AsList();
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var command = BuildCommand(sql, parameters, commandType, commandTimeout, cancellationToken);
        return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<T>(command);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var command = BuildCommand(sql, parameters, commandType, commandTimeout, cancellationToken);
        return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var command = BuildCommand(sql, parameters, commandType, commandTimeout, cancellationToken);
        return await _unitOfWork.Connection.ExecuteAsync(command);
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var command = BuildCommand(sql, parameters, commandType, commandTimeout, cancellationToken);
        return await _unitOfWork.Connection.ExecuteScalarAsync<T>(command);
    }

    private CommandDefinition BuildCommand(
        string sql,
        object? parameters,
        CommandType commandType,
        int? commandTimeout,
        CancellationToken cancellationToken)
        => new(
            sql,
            parameters,
            _unitOfWork.CurrentTransaction,
            commandTimeout,
            commandType,
            cancellationToken: cancellationToken);
}
