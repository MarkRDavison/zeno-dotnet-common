namespace mark.davison.common.server.EventDriven;

public abstract class ChangesetQueue : IChangesetQueue
{
    private readonly object _lock = new object();

    private readonly LinkedList<EntityChangeset> _changesets = new();
    private readonly LinkedList<EntityChangeset> _barriers = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected ChangesetQueue(
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task ProcessNextBarrier()
    {
        if (HasPendingBarrier())
        {
            return ProcessToBarrier(_barriers.First!.Value.Id);
        }

        return Task.CompletedTask;
    }

    public async Task ProcessToBarrier(Guid barrierId)
    {
        if (!_barriers.Any(_ => _.Id == barrierId))
        {
            return;
        }

        var changes = PeekToNextBarrier();

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(changes);

        if (!ChangesetUtilities.ValidateChangesets(consolidated))
        {
            throw new InvalidOperationException("Cannot process an invalid set of changes");
        }

        var success = await ProcessChanges(changes);

        if (!success)
        {
            throw new InvalidOperationException("Failed to process set of changes");
        }

        PopToNextBarrier();
    }

    protected abstract Task<bool> ProcessChanges(List<EntityChangeset> changes);

    public void Append(EntityChangeset changeset)
    {
        lock (_lock)
        {
            if (changeset.EntityChangeType == EntityChangeType.Barrier)
            {
                if (_changesets.Any())
                {
                    _changesets.AddLast(changeset);
                    _barriers.AddLast(changeset);
                }
            }
            else
            {
                _changesets.AddLast(changeset);
            }
        }
    }

    public bool HasPendingBarrier() => _barriers.Any();

    public List<EntityChangeset> PeekToNextBarrier()
    {
        var list = new List<EntityChangeset>();

        if (_barriers.Any())
        {
            var firstBarrierId = _barriers.First!.Value.Id;

            var curr = _changesets.First;

            while (curr != null && curr.Value.Id != firstBarrierId)
            {
                list.Add(curr.Value);
                curr = curr.Next;
            }
        }

        return list;
    }

    public List<EntityChangeset> PopToNextBarrier()
    {
        var list = new List<EntityChangeset>();

        if (_barriers.Any())
        {
            lock (_lock)
            {
                var firstBarrierId = _barriers.First!.Value.Id;

                var curr = _changesets.First;

                while (curr != null && curr.Value.Id != firstBarrierId)
                {
                    list.Add(curr.Value);
                    curr = curr.Next;
                    _changesets.RemoveFirst();
                }

                _barriers.RemoveFirst();
            }
        }

        return list;
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

    private (List<TEntity>, HashSet<Guid>) FindLocallyAddedOrDeletedEntities<TEntity>(List<EntityChangeset> consolidated) where TEntity : BaseEntity, new()
    {
        var removed = consolidated
            .Where(_ => _.EntityChangeType == EntityChangeType.Delete)
            .Select(_ => _.EntityId)
            .ToHashSet();

        var results = consolidated
            .Select(_ => ChangesetUtilities.Apply<TEntity>(new TEntity()
            {
                Id = _.EntityId
            }, _)).Where(_ => _ != null)
            .Cast<TEntity>()
            .ToList();

        foreach (var r in results)
        {
            removed.Remove(r.Id);
        }

        return (results, removed);
    }

    public Task<List<T>> GetEntitiesAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(null, null, string.Empty, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(predicate, null, string.Empty, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(null, includes, string.Empty, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(string includes, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(null, null, includes, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? includes, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(predicate, includes, string.Empty, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>>? predicate, string includes, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<T, T>(predicate, null, includes, null, cancellationToken);
    }

    private async Task<List<TProjection>> GetEntitiesAsyncInternal<TEntity, TProjection>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>[]? includes, string includesString, Expression<Func<TEntity, TProjection>>? projection, CancellationToken cancellationToken = default) where TEntity : BaseEntity, new()
    {
        var typeName = typeof(TEntity).AssemblyQualifiedName;

        var relatedChangesets = _changesets.Where(_ => _.Type == typeName).ToList();

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(relatedChangesets);
        if (!ChangesetUtilities.ValidateChangesets(consolidated))
        {
            throw new InvalidOperationException("Cannot process an invalid set of changes");
        }

        var predicateFunc = predicate?.Compile();

        var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadonlyRepository>();

        await using (repository.BeginTransaction())
        {
            var (localExisting, localRemoved) = FindLocallyAddedOrDeletedEntities<TEntity>(consolidated);
            List<TEntity> remote;

            if (predicateFunc != null)
            {
                localExisting = localExisting.Where(predicateFunc).ToList();
            }

            if (includes == null)
            {
                remote = await repository.GetEntitiesAsync<TEntity>(predicate, includesString, cancellationToken);
            }
            else
            {
                remote = await repository.GetEntitiesAsync<TEntity>(predicate, includes, cancellationToken);
            }

            var remoteAppliedEntities = new List<TEntity>();

            foreach (var remoteEntity in remote)
            {
                var remoteApplied = remoteEntity;

                if (localRemoved.Contains(remoteApplied.Id))
                {
                    continue;
                }

                foreach (var cs in consolidated)
                {
                    remoteApplied = ChangesetUtilities.Apply<TEntity>(remoteApplied, cs);
                }

                remoteAppliedEntities.Add(remoteApplied);
            }

            var remoteIds = remoteAppliedEntities.Select(_ => _.Id).ToHashSet();

            var results = remoteAppliedEntities.Concat(localExisting.Where(_ => !remoteIds.Contains(_.Id))).ToList();

            var projectionFunc = projection?.Compile();

            if (projectionFunc == null)
            {
                return results.OfType<TProjection>().ToList();
            }

            return results.Select(projectionFunc).ToList();
        }
    }

    public async Task<T?> GetEntityAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(_ => _.Id == id, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public async Task<T?> GetEntityAsync<T>(Guid id, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(_ => _.Id == id, include, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public async Task<T?> GetEntityAsync<T>(Guid id, string include, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(_ => _.Id == id, include, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public async Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(predicate, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public async Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>[]? include, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(predicate, include, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public async Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, string include, CancellationToken cancellationToken = default) where T : BaseEntity, new()
    {
        var entities = await GetEntitiesAsync<T>(predicate, include, cancellationToken);
        return entities.FirstOrDefault();// TODO: Dont just use predicate for the get single.
    }

    public Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>[]? includes, Expression<Func<TEntity, TProjection>>? projection, CancellationToken cancellationToken = default) where TEntity : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<TEntity, TProjection>(predicate, includes, string.Empty, projection, cancellationToken);
    }

    public Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(Expression<Func<TEntity, bool>>? predicate, string includes, Expression<Func<TEntity, TProjection>>? projection, CancellationToken cancellationToken = default) where TEntity : BaseEntity, new()
    {
        return GetEntitiesAsyncInternal<TEntity, TProjection>(predicate, null, includes, projection, cancellationToken);
    }
}
