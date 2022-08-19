using mark.davison.common.Repository;

namespace mark.davison.common.server.abstractions.Repository;

public interface IHttpRepository
{
    Task<T?> GetEntityAsync<T>(Guid id, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity;
    Task<T?> GetEntityAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity;
    Task<List<T>> GetEntitiesAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity;
    Task<List<T>> GetEntitiesAsync<T>(string path, QueryParameters query, HeaderParameters header, CancellationToken cancellationToken);
    Task<T?> UpsertEntityAsync<T>(T entity, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity;
    Task<List<T>> UpsertEntitiesAsync<T>(List<T> entities, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity;
}