using Microsoft.EntityFrameworkCore.Storage;

namespace mark.davison.common.persistence.Repository;

public abstract class RepositoryBase<TContext> : IRepository
    where TContext : DbContext
{
    class RepositoryTransaction : IAsyncDisposable
    {
        private bool disposedValue;
        private bool _completed;
        private readonly RepositoryBase<TContext> _repository;
        private readonly TContext _context;
        private readonly IDbContextTransaction _transaction;

        public int Depth { get; set; }

        public RepositoryTransaction(RepositoryBase<TContext> repository, TContext context)
        {
            _repository = repository;
            _context = context;

            _transaction = _context.Database.BeginTransaction();

            Depth = 1;
        }

        public async ValueTask DisposeAsync()
        {
            Depth--;
            if (Depth > 0)
            {
                return;
            }
            if (!disposedValue)
            {
                disposedValue = true;
                if (_completed)
                {
                    _repository.CleanupTransaction();
                }
                else
                {
                    try
                    {
                        await _repository.ContextSaveChanges(_context, CancellationToken.None);
                        await _transaction.CommitAsync();
                        _repository.CleanupTransaction();
                    }
                    catch (Exception)
                    {
                        await _transaction.RollbackAsync();
                        _repository.CleanupTransaction();
                        throw;
                    }
                }
            }
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            _completed = true;
        }

        public async Task CommitAsync()
        {
            await _repository.ContextSaveChanges(_context, CancellationToken.None);
            await _transaction.CommitAsync();
            _completed = true;
        }
    }

    private readonly ILogger<RepositoryBase<TContext>> _logger;
    private readonly IDbContextFactory<TContext> _dbContextFactory;
    private TContext? _context = null;
    private RepositoryTransaction? _activeTransaction = null;

    protected RepositoryBase(
        IDbContextFactory<TContext> dbContextFactory,
        ILogger<RepositoryBase<TContext>> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }


    public Task<List<T>> GetEntitiesAsync<T>(
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T>(null, (Expression<Func<T, object>>[]?)null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(
        Expression<Func<T, bool>>? predicate,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T>(predicate, (Expression<Func<T, object>>[]?)null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(
        Expression<Func<T, object>>[]? includes,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T>(null, includes, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(
        string includes,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T>(null, includes, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>>[]? includes,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T, T>(predicate, includes, null, cancellationToken);
    }

    public Task<List<T>> GetEntitiesAsync<T>(
        Expression<Func<T, bool>>? predicate,
        string includes,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntitiesAsync<T, T>(predicate, includes, null, cancellationToken);
    }

    public async Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>[]? includes,
        Expression<Func<TEntity, TProjection>>? projection,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity, new()
    {
        return await QueryUnitOfWork(predicate, includes, projection, cancellationToken).ToListAsync(cancellationToken);
    }

    public async Task<List<TProjection>> GetEntitiesAsync<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate,
        string includes,
        Expression<Func<TEntity, TProjection>>? projection,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity, new()
    {
        return await QueryUnitOfWork(predicate, includes, projection, cancellationToken).ToListAsync(cancellationToken);
    }

    public Task<T?> GetEntityAsync<T>(
        Guid id,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntityAsync<T>(id, (Expression<Func<T, object>>[]?)null, cancellationToken);
    }
    public async Task<bool> EntityExistsAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return null != await GetEntityAsync<T>(id, cancellationToken);
    }

    public Task<T?> GetEntityAsync<T>(
        Guid id,
        Expression<Func<T, object>>[]? include,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntityAsync<T>(_ => _.Id == id, include, cancellationToken);
    }
    public Task<T?> GetEntityAsync<T>(
        Guid id,
        string include,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntityAsync<T>(_ => _.Id == id, include, cancellationToken);
    }

    public Task<T?> GetEntityAsync<T>(
        Expression<Func<T, bool>>? predicate,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return GetEntityAsync<T>(predicate, (Expression<Func<T, object>>[]?)null, cancellationToken);
    }
    public async Task<bool> EntityExistsAsync<T>(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return null != await GetEntityAsync<T>(predicate, cancellationToken);
    }

    public async Task<T?> GetEntityAsync<T>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>>[]? include,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return await QueryUnitOfWork(predicate, include, _ => _, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<T?> GetEntityAsync<T>(Expression<Func<T, bool>>? predicate, string include, CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return await QueryUnitOfWork(predicate, include, _ => _, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> UpsertEntitiesAsync<T>(
        List<T> entities,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        //Check that there are no entities in the list with duplicate Id's that are not Guid.Empty
        var containsInvalidDuplicates = entities
            .Where(_ => !_.Id.Equals(Guid.Empty))
            .GroupBy(_ => _.Id)
            .Any(_ => _.Count() > 1);

        if (containsInvalidDuplicates)
        {
            throw new InvalidOperationException("Duplicate entities detected.");
        }

        var set = _context.Set<T>();

        var addBatchItems = entities
            .Where(_ => _.Id.Equals(Guid.Empty))
            .ToList();

        set.AddRange(addBatchItems);

        var upsertBatchItems = entities
            .Where(_ => !_.Id.Equals(Guid.Empty))
            .ToList();
        var upsertBatchItemsIds = upsertBatchItems
            .Select(_ => _.Id)
            .ToArray();

        var existingBatchItems = await set
            .Where(_ => upsertBatchItemsIds.Contains(_.Id))
            .ToListAsync(cancellationToken);

        foreach (var upsertBatchItem in upsertBatchItems)
        {
            upsertBatchItem.LastModified = DateTime.UtcNow;
            var existingBatchItem = existingBatchItems.Find(_ => _.Id.Equals(upsertBatchItem.Id));
            if (existingBatchItem == null)
            {
                upsertBatchItem.Created = DateTime.UtcNow;
                set.Add(upsertBatchItem);
            }
            else
            {
                _context.Entry(existingBatchItem).CurrentValues.SetValues(upsertBatchItem);
            }
        }

        return entities;
    }

    [ExcludeFromCodeCoverage]
    private async Task ContextSaveChanges(TContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("ContextSaveChanges - {0}", e);
            throw;
        }
    }

    public async Task<T?> UpsertEntityAsync<T>(
        T entity,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var set = _context.Set<T>();
        var existingEntity = await set.FindAsync(new object[] { entity.Id }, cancellationToken: cancellationToken);
        if (existingEntity == null)
        {
            entity.Created = DateTime.UtcNow;
            entity.LastModified = DateTime.UtcNow;
            set.Add(entity);
        }
        else
        {
            //SetValues will only mark as modified the properties that have different
            //values to those in the tracked entity. This means that when the update
            //is sent, only those columns that have actually changed will be updated.
            //(And if nothing has changed, then no update will be sent at all.)
            entity.LastModified = DateTime.UtcNow;
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        return entity;
    }

    public Task<T?> DeleteEntityAsync<T>(
        T entity,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        return DeleteUnitOfWorkAsync<T>(entity, cancellationToken);
    }

    public async Task<T?> DeleteEntityAsync<T>(
        Guid id,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        var entity = await GetEntityAsync<T>(id, cancellationToken);
        if (entity == null)
        {
            return null;
        }
        return await DeleteUnitOfWorkAsync<T>(entity, cancellationToken);
    }

    public async Task<List<T>> DeleteEntitiesAsync<T>(
        List<T> entities,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var itemsToDelete = entities
            .Where(_ => !_.Id.Equals(Guid.Empty))
            .ToList();
        var itemsToDeleteIds = itemsToDelete
            .Select(_ => _.Id)
            .ToArray();

        var entitiesToDelete = await GetEntitiesAsync<T>(_ => itemsToDeleteIds.Contains(_.Id), cancellationToken);

        return ContextRemoveRangeSaveChanges(_context, itemsToDelete, entitiesToDelete, cancellationToken);
    }

    public async Task<List<T>> DeleteEntitiesAsync<T>(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, new()
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var entitiesToDelete = await GetEntitiesAsync<T>(_ => ids.Contains(_.Id), cancellationToken);

        return await DeleteEntitiesAsync<T>(entitiesToDelete, cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    private List<T> ContextRemoveRangeSaveChanges<T>(TContext context, List<T> itemsToDelete, List<T> entitiesToDelete, CancellationToken cancellationToken) where T : BaseEntity, new()
    {
        try
        {
            context.RemoveRange(entitiesToDelete);

            return itemsToDelete;
        }
        catch (Exception e)
        {
            _logger.LogError("DeleteEntitiesAsync<{0}> - {1}", nameof(T), e);
            throw;
        }
    }

    private IQueryable<TProjection> QueryUnitOfWork<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        Expression<Func<TEntity, TProjection>>? projection = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var set = _context.Set<TEntity>();

        var query = set.Where(_ => true).AsNoTracking();

        query = AttachIncludes<TEntity>(query, includes);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return QueryApplyProjection(projection, query);
    }

    [ExcludeFromCodeCoverage]
    private IQueryable<TProjection> QueryApplyProjection<TEntity, TProjection>(Expression<Func<TEntity, TProjection>>? projection, IQueryable<TEntity> query) where TEntity : BaseEntity
    {
        try
        {
            IQueryable<TProjection> result;

            if (projection != null)
            {
                result = query
                    .Select(projection);
            }
            else
            {
                result = query
                    .OfType<TProjection>();
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("QueryApplyProjection<{0}> - {1}", nameof(TEntity), e);
            throw;
        }
    }

    private IQueryable<TProjection> QueryUnitOfWork<TEntity, TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        string includes = "",
        Expression<Func<TEntity, TProjection>>? projection = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var set = _context.Set<TEntity>();

        var query = set.Where(_ => true).AsNoTracking();

        query = AttachIncludes<TEntity>(query, includes?.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return QueryApplyProjection(projection, query);
    }

    private async Task<TEntity?> DeleteUnitOfWorkAsync<TEntity>(
        TEntity item,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity, new()
    {
        if (_context == null) throw new InvalidOperationException("Cannot interact with repository without starting a transaction");

        var existingItem = await GetEntityAsync<TEntity>(item.Id, (Expression<Func<TEntity, object>>[]?)null, cancellationToken);

        if (existingItem != null)
        {
            var entry = _context.Entry(item);
            if (entry.State == EntityState.Deleted)
            {
                return null;
            }
            entry.State = EntityState.Deleted;
        }
        else
        {
            return null;
        }

        return existingItem;
    }

    [ExcludeFromCodeCoverage]
    private static IQueryable<TEntity> AttachIncludes<TEntity>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, object>>[]? includes)
        where TEntity : BaseEntity
    {
        if (includes == null)
        {
            return query;
        }

        foreach (var include in includes)
        {
            if (include.Body is not MethodCallExpression expression)
            {
                query = query.Include(include);
                continue;
            }

            var arguments = expression.Arguments;

            if (arguments.Count <= 1)
            {
                query = query.Include(include);
                continue;
            }

            var parts = arguments.Select(ExtractExpressionParts);

            var navigationPath = string.Join(".", parts);
            query = query.Include(navigationPath);
        }

        return query;
    }

    [ExcludeFromCodeCoverage]
    private static string ExtractExpressionParts(Expression expression)
    {
        var startIndex = expression.ToString().IndexOf('.') + 1;
        return expression.ToString()[startIndex..];
    }

    private static IQueryable<TEntity> AttachIncludes<TEntity>(
        IQueryable<TEntity> query,
        string[]? includes)
        where TEntity : BaseEntity
    {
        if (includes == null)
        {
            return query;
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }

    public IAsyncDisposable BeginTransaction()
    {
        if (_activeTransaction != null && _context != null)
        {
            _activeTransaction.Depth++;
            return _activeTransaction;
        }

        if (_activeTransaction != null || _context != null)
        {
            throw new InvalidOperationException("Cannot start a transaction while one is underway");
        }

        _context = _dbContextFactory.CreateDbContext();
        _activeTransaction = new RepositoryTransaction(this, _context);

        return _activeTransaction;
    }
    public async Task RollbackTransactionAsync()
    {
        if (_context == null || _activeTransaction == null) throw new InvalidOperationException("Cannot rollback a transaction that hasn't been started");

        await _activeTransaction.RollbackAsync();

        CleanupTransaction();
    }
    public async Task CommitTransactionAsync()
    {
        if (_context == null || _activeTransaction == null) throw new InvalidOperationException("Cannot rollback a transaction that hasn't been started");

        await _activeTransaction.CommitAsync();

        CleanupTransaction();
    }

    protected void CleanupTransaction()
    {
        _context = null;
        _activeTransaction = null;
    }
}