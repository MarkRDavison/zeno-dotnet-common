namespace mark.davison.common.server.abstractions.Repository;

public interface IRepository
{
    Task<List<T>> GetEntitiesAsync<T>(CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>[]? includes, Expression<Func<TEntity, TProjection>>? projection,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    Task<T?> GetEntityAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<T?> GetEntityAsync<T>(Guid id, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<T>> UpsertEntitiesAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<T?> UpsertEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<T?> DeleteEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    Task<List<T>> DeleteEntitiesAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : BaseEntity;
}