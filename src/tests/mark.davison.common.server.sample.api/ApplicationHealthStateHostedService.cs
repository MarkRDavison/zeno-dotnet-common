namespace mark.davison.common.server.sample.api;

public class ApplicationHealthStateHostedService : ApiApplicationHealthStateHostedService<TestDbContext, AppSettings>
{
    public ApplicationHealthStateHostedService(IApplicationHealthState applicationHealthState, IHostApplicationLifetime hostApplicationLifetime, IDbContextFactory<TestDbContext> dbContextFactory, IOptions<AppSettings> appSettings) : base(applicationHealthState, hostApplicationLifetime, appSettings, dbContextFactory)
    {
    }

    protected override async Task InitDatabaseProduction(TestDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
