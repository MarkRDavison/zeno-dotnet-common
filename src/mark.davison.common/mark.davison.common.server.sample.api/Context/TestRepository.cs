namespace mark.davison.common.server.sample.api.Context;

public class TestRepository : RepositoryBase<TestDbContext>
{
    public TestRepository(
        IDbContextFactory<TestDbContext> dbContextFactory,
        ILogger<RepositoryBase<TestDbContext>> logger
    ) : base(
        dbContextFactory,
        logger)
    {
    }
}
