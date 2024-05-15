namespace mark.davison.common.server.abstractions.Repository;

[Obsolete]
public interface IReadonlyRepository : IReadRepository
{
    IAsyncDisposable BeginTransaction();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
}
