namespace mark.davison.common.server.test.Persistence;

public static class DbContextHelpers
{
    public static IDbContext<TContext> CreateInMemory<TContext>(
        Func<DbContextOptions, TContext> creator)
        where TContext : DbContextBase<TContext>
    {
        return CreateInMemory<TContext>(creator, $"{Guid.NewGuid()}.db");
    }

    public static IDbContext<TContext> CreateInMemory<TContext>(
        Func<DbContextOptions, TContext> creator,
        string databaseName)
        where TContext : DbContextBase<TContext>
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .ConfigureWarnings((WarningsConfigurationBuilder _) => _.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return creator(optionsBuilder.Options);
    }
}
