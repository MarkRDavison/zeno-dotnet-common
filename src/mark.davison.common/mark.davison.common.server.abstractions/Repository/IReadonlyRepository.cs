namespace mark.davison.common.server.abstractions.Repository;

public interface IReadonlyRepository : IReadRepository
{
    IAsyncDisposable BeginTransaction();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
}
