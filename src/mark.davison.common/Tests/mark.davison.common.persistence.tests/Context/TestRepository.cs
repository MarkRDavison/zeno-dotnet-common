namespace mark.davison.common.persistence.tests.Context;

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
