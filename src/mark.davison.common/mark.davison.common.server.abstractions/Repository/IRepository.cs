namespace mark.davison.common.server.abstractions.Repository;

public interface IRepository : IReadonlyRepository
{
    Task<List<T>> UpsertEntitiesAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> UpsertEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> DeleteEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<T?> DeleteEntityAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> DeleteEntitiesAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();

    Task<List<T>> DeleteEntitiesAsync<T>(List<Guid> ids, CancellationToken cancellationToken = default)
        where T : BaseEntity, new();
}