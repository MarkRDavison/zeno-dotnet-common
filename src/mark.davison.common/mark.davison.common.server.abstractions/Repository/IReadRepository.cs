namespace mark.davison.common.server.abstractions.Repository;

public interface IReadRepository
{
    Task<List<T>> GetEntitiesAsync<T>(CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> GetEntitiesAsync<T>(string includes, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, string includes, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> GetEntityAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> GetEntityAsync<T>(Guid id, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();
    Task<T?> GetEntityAsync<T>(Guid id, string include, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();
    Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, string include, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>[]? includes,
        Expression<Func<TEntity, TProjection>>? projection,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity, new();
    Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate,
        string includes,
        Expression<Func<TEntity, TProjection>>? projection,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity, new();
}
