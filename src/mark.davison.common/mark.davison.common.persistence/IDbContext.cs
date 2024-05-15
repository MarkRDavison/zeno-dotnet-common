namespace mark.davison.common.persistence;

public interface ICommonDbContextTransaction<TContext> : IDisposable where TContext : DbContext
{
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);

    bool RolledBack { get; }
}

public interface IDbContext<TContext> where TContext : DbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class;

    EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<EntityEntry<TEntity>?> RemoveAsync<TEntity>(Guid id, CancellationToken cancellationToken) where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);

    Task<ICommonDbContextTransaction<TContext>> BeginTransactionAsync(CancellationToken cancellationToken);
}
