namespace mark.davison.common.server.EventDriven;

public class ChangesetGroup : IChangesetGroup
{
    private readonly IChangesetQueue _queue;
    private ChangesetGroupTransaction? _activeTransaction = null;
    private readonly List<EntityChangeset> _changes = new();

    public ChangesetGroup(
        IChangesetQueue queue
    )
    {
        _queue = queue;
    }

    public void Rollback()
    {
        _changes.Clear();
    }

    private void PushToQueue()
    {
        if (!_changes.Any())
        {
            return;
        }

        foreach (var cs in _changes)
        {
            _queue.Append(cs);
        }

        _queue.Append(new() { EntityChangeType = EntityChangeType.Barrier });

        _changes.Clear();
    }

    public void Append(EntityChangeset changeset)
    {
        _changes.Add(changeset);
    }

    public void Add<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
    {
        var cs = ChangesetUtilities.GenerateChangeset<TEntity>(entity);

        Append(cs);
    }

    public void Modify<TEntity>(TEntity existing, TEntity updated) where TEntity : BaseEntity, new()
    {
        var cs = ChangesetUtilities.GenerateChangeset<TEntity>(existing, updated);

        Append(cs);
    }

    public void Delete<TEntity>(Guid id) where TEntity : BaseEntity, new()
    {
        var cs = ChangesetUtilities.GenerateChangeset<TEntity>(id);

        Append(cs);
    }

    public IChangesetGroupTransaction BeginTransaction()
    {
        if (_activeTransaction == null)
        {
            _activeTransaction = new ChangesetGroupTransaction(this);
        }
        else
        {
            _activeTransaction.TransactionDepth++;
        }

        return _activeTransaction;
    }

    public void EndTransaction()
    {
        PushToQueue();

        _activeTransaction = null;
    }
}
