namespace mark.davison.common.server.abstractions.EventDriven;

public interface IChangesetGroup
{
    public IChangesetGroupTransaction BeginTransaction();
    public void EndTransaction();
    public void Rollback();
    public void Append(EntityChangeset changeset);
    public void Add<TEntity>(TEntity entity) where TEntity : BaseEntity, new();
    public void Modify<TEntity>(TEntity existing, TEntity updated) where TEntity : BaseEntity, new();
    public void Delete<TEntity>(Guid id) where TEntity : BaseEntity, new();
}
