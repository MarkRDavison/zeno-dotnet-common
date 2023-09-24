namespace mark.davison.common.server.EventDriven;

public class RepositoryBackedChangesetQueue : ChangesetQueue
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RepositoryBackedChangesetQueue(
        IServiceScopeFactory serviceScopeFactory
    ) : base
    (
        serviceScopeFactory
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task<bool> ProcessChanges(List<EntityChangeset> changes)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        await using (repository.BeginTransaction())
        {
            try
            {
                foreach (var cs in changes)
                {
                    switch (cs.EntityChangeType)
                    {
                        case EntityChangeType.Add:
                            await ApplyChangsetAsync(nameof(AddEntityAsync), cs, repository);
                            break;
                        case EntityChangeType.Modify:
                            await ApplyChangsetAsync(nameof(ModifyEntityAsync), cs, repository);
                            break;
                        case EntityChangeType.Delete:
                            await ApplyChangsetAsync(nameof(DeleteEntityAsync), cs, repository);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                await repository.RollbackTransactionAsync();
                throw;
            }
        }

        return true;
    }

    private async Task ApplyChangsetAsync(string methodName, EntityChangeset changeset, IRepository repository)
    {
        var entityType = Type.GetType(changeset.Type);

        if (entityType == null)
        {
            throw new InvalidOperationException("Invalid entity type");
        }

        var methodInfo = typeof(RepositoryBackedChangesetQueue)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(_ => _.Name == methodName && _.IsGenericMethodDefinition)
            .FirstOrDefault();

        if (methodInfo == null)
        {
            throw new InvalidOperationException("Invalid method");
        }

        var method = methodInfo.MakeGenericMethod(entityType);

        if (method == null)
        {
            throw new InvalidOperationException("Invalid generic method");
        }

        await (Task)method.Invoke(this, new object?[] { changeset, repository })!;
    }

    private async Task AddEntityAsync<TEntity>(EntityChangeset changeset, IRepository repository) where TEntity : BaseEntity, new()
    {
        var entity = ChangesetUtilities.Apply<TEntity>(null, changeset);

        if (entity != null)
        {
            await repository.UpsertEntityAsync<TEntity>(entity, CancellationToken.None);
        }
    }

    private async Task ModifyEntityAsync<TEntity>(EntityChangeset changeset, IRepository repository) where TEntity : BaseEntity, new()
    {
        var entity = await repository.GetEntityAsync<TEntity>(changeset.EntityId, CancellationToken.None);

        entity = ChangesetUtilities.Apply<TEntity>(entity, changeset);

        if (entity != null)
        {
            await repository.UpsertEntityAsync<TEntity>(entity, CancellationToken.None);
        }
    }

    private async Task DeleteEntityAsync<TEntity>(EntityChangeset changeset, IRepository repository) where TEntity : BaseEntity, new()
    {
        var entity = await repository.GetEntityAsync<TEntity>(changeset.EntityId, CancellationToken.None);

        if (entity != null)
        {
            await repository.DeleteEntityAsync<TEntity>(entity, CancellationToken.None);
        }
    }
}
