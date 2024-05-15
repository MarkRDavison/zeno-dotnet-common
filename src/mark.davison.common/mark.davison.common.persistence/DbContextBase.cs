namespace mark.davison.common.persistence;

public class DbContextTransaction<TContext> : ICommonDbContextTransaction<TContext> where TContext : DbContext
{
    private readonly DbContextBase<TContext> _context;
    private readonly IDbContextTransaction? _rootTransaction;
    private bool _disposedValue;

    public DbContextTransaction(
        DbContextBase<TContext> context,
        IDbContextTransaction? rootTransaction)
    {
        _context = context;
        _rootTransaction = rootTransaction;

        _context.RefCount++;
    }

    public bool RolledBack => _context.RolledBack;

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (RolledBack)
        {
            throw new InvalidOperationException("Cannot commit a transaction that has already been rolled back");
        }

        _context.RefCount--;

        if (_rootTransaction != null)
        {
            if (_context.RefCount == 0)
            {
                await _rootTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        _context.RolledBack = true;
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _rootTransaction?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public abstract class DbContextBase<TContext> : DbContext, IDbContext<TContext> where TContext : DbContext
{
    private DbContextTransaction<TContext>? _root = null;

    public int RefCount { get; set; }
    public bool RolledBack { get; set; }

    protected DbContextBase(DbContextOptions options) : base(options)
    {

    }

    public async Task<EntityEntry<TEntity>?> RemoveAsync<TEntity>(Guid id, CancellationToken cancellationToken) where TEntity : BaseEntity
    {
        var entity = await FindAsync<TEntity>([id], cancellationToken: cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return Remove(entity);
    }

    public async Task<ICommonDbContextTransaction<TContext>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_root == null)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            _root = new DbContextTransaction<TContext>(this, transaction);

            return _root;
        }

        return new DbContextTransaction<TContext>(this, null);
    }
}
