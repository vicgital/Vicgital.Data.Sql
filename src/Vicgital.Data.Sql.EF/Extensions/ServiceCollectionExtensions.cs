using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Vicgital.Data.Sql.Abstractions;
using Vicgital.Data.Sql.Connections;
using Vicgital.Data.Sql.Dapper;
using Vicgital.Data.Sql.EntityFrameworkCore;

namespace Vicgital.Data.Sql.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TContext"/> against SQL Server plus the shared repository,
    /// unit-of-work and Dapper abstractions on top of it. Call once per DbContext in a service's
    /// infrastructure layer composition root.
    /// </summary>
    public static IServiceCollection AddVicgitalDataSql<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptionsAction));

        // Lets the open-generic Repository<TEntity,TKey> below resolve the concrete TContext
        // through the common DbContext base type.
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped<IDapperQueryExecutor, DapperQueryExecutor>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

        return services;
    }
}
