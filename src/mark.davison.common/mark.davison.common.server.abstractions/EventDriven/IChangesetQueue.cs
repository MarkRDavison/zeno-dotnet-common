namespace mark.davison.common.server.abstractions.EventDriven;

public interface IChangesetQueue : IReadRepository
{
    public Task ProcessNextBarrier();
    public Task ProcessToBarrier(Guid barrierId);
    public void Append(EntityChangeset changeset);
    public List<EntityChangeset> PeekToNextBarrier();
    public List<EntityChangeset> PopToNextBarrier();
    public bool HasPendingBarrier();
    public void Add<TEntity>(TEntity entity) where TEntity : BaseEntity, new();
    public void Modify<TEntity>(TEntity existing, TEntity updated) where TEntity : BaseEntity, new();
    public void Delete<TEntity>(Guid id) where TEntity : BaseEntity, new();
}
