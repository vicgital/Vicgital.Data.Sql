using Microsoft.Extensions.DependencyInjection;
using Vicgital.Data.Sql.Abstractions;
using Vicgital.Data.Sql.Ado;
using Vicgital.Data.Sql.Connections;
using Vicgital.Data.Sql.Dapper;

namespace Vicgital.Data.Sql.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Dapper-only path for services with no EF Core DbContext: an
    /// <see cref="IDbConnectionFactory"/>, an ADO.NET-native <see cref="IUnitOfWork"/>
    /// (<see cref="AdoUnitOfWork"/>), and <see cref="IDapperQueryExecutor"/>. Does not register
    /// <see cref="IRepository{TEntity,TKey}"/>, since that requires a DbContext.
    /// </summary>
    public static IServiceCollection AddVicgitalDataSqlDapper(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddScoped<IUnitOfWork, AdoUnitOfWork>();
        services.AddScoped<IDapperQueryExecutor, DapperQueryExecutor>();

        return services;
    }
}
