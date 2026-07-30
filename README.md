# Vicgital.Data.Sql

Shared .NET library for SQL Server data access, built on Entity Framework Core and Dapper.
Consumed by the infrastructure layer of Vicgital's gRPC services so repository, unit-of-work,
and transaction-coordination code isn't reimplemented per service.

## Design

- **Generic repository, EF Core primary.** `IRepository<TEntity, TKey>` and its base
  implementation `Repository<TEntity, TKey>` cover common CRUD/query needs for simple entities.
  Extend `Repository<TEntity, TKey>` per aggregate when you need entity-specific queries.
- **Dapper as an escape hatch.** `IDapperQueryExecutor` runs hand-written SQL for complex reads,
  reports, and bulk operations. When an EF `DbContext` is in play, it shares the same connection
  and transaction as the unit of work.
- **One unit of work, two implementations.** `IUnitOfWork` coordinates commits/rollbacks across
  EF Core and Dapper. `UnitOfWork<TContext>` backs it with a `DbContext`; `AdoUnitOfWork` backs it
  with a plain ADO.NET connection/transaction for services that don't use EF at all.

## Install

Packages restore from both `nuget.org` and Vicgital's GitHub Packages feed (see `nuget.config`).
For the GitHub source, set the `GH_PACKAGE_TOKEN` environment variable to a PAT with
`read:packages` scope.

```xml
<PackageReference Include="Vicgital.Data.Sql" Version="1.0.0" />
```

## Usage

### EF Core (with a generic repository)

```csharp
// composition root
services.AddVicgitalDataSql<OrdersDbContext>(connectionString);
```

`AddVicgitalDataSql<TContext>` registers `TContext` against SQL Server, `IUnitOfWork` as
`UnitOfWork<TContext>`, the open-generic `IRepository<,>` as `Repository<,>`, and
`IDapperQueryExecutor`/`IDbConnectionFactory` sharing the same connection string.

Consume the generic repository directly for simple entities:

```csharp
public sealed class ProductService
{
    private readonly IRepository<Product, Guid> _products;

    public ProductService(IRepository<Product, Guid> products) => _products = products;

    public Task<Product?> GetAsync(Guid id, CancellationToken ct) => _products.GetByIdAsync(id, ct);
}
```

Extend it when an aggregate needs entity-specific queries:

```csharp
public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<IReadOnlyList<Order>> GetPendingAsync(CancellationToken cancellationToken = default);
}

public sealed class OrderRepository : Repository<Order, Guid>, IOrderRepository
{
    public OrderRepository(DbContext context) : base(context) { }

    public Task<IReadOnlyList<Order>> GetPendingAsync(CancellationToken cancellationToken = default)
        => Query.Where(o => o.Status == OrderStatus.Pending).ToListAsync(cancellationToken);
}
```

```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```

Mix EF writes with Dapper reads in one transaction via the shared `IUnitOfWork`:

```csharp
await unitOfWork.BeginTransactionAsync(ct);
try
{
    await orderRepository.AddAsync(order, ct);
    await unitOfWork.SaveChangesAsync(ct);

    var summary = await dapper.QuerySingleOrDefaultAsync<OrderSummary>(
        "SELECT * FROM OrderSummaries WHERE OrderId = @Id", new { order.Id }, cancellationToken: ct);

    await unitOfWork.CommitAsync(ct);
}
catch
{
    await unitOfWork.RollbackAsync(ct);
    throw;
}
```

### Dapper only (no EF Core, no DbContext)

```csharp
// composition root
services.AddVicgitalDataSqlDapper(connectionString);
```

This registers `IDbConnectionFactory`, `IUnitOfWork` as `AdoUnitOfWork` (a plain ADO.NET
connection/transaction), and `IDapperQueryExecutor` — no EF Core assemblies or `DbContext`
required. `IRepository<,>` is **not** registered in this mode: its query methods rely on EF
translating LINQ expressions to SQL, which Dapper doesn't do. Instead, write a purpose-built
interface per aggregate against `IDapperQueryExecutor`:

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}

public sealed class OrderRepository : IOrderRepository
{
    private readonly IDapperQueryExecutor _dapper;

    public OrderRepository(IDapperQueryExecutor dapper) => _dapper = dapper;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dapper.QuerySingleOrDefaultAsync<Order>(
            "SELECT Id, CustomerId, Status, CreatedAtUtc FROM Orders WHERE Id = @Id",
            new { Id = id },
            cancellationToken: cancellationToken);

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => _dapper.ExecuteAsync(
            """
            INSERT INTO Orders (Id, CustomerId, Status, CreatedAtUtc)
            VALUES (@Id, @CustomerId, @Status, @CreatedAtUtc);
            """,
            order,
            cancellationToken: cancellationToken);
}
```

```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```

`IUnitOfWork.SaveChangesAsync` is a no-op under `AdoUnitOfWork` — there's no change tracker in
Dapper-only mode, so writes commit as soon as `ExecuteAsync` runs. Use
`BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` to coordinate multiple statements/repository
calls atomically, the same way as the EF Core mode.

## Project layout

```
src/Vicgital.Data.Sql/
  Abstractions/          IRepository, IUnitOfWork, IDapperQueryExecutor, IDbConnectionFactory
  EntityFrameworkCore/    Repository<TEntity,TKey>, UnitOfWork<TContext>
  Dapper/                 DapperQueryExecutor
  Ado/                    AdoUnitOfWork (EF-free unit of work)
  Connections/            SqlConnectionFactory
  Extensions/             AddVicgitalDataSql<TContext>, AddVicgitalDataSqlDapper
```

## Requirements

- .NET 10
- SQL Server (via `Microsoft.Data.SqlClient`)
