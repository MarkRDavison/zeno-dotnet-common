namespace mark.davison.example.api;

public class ApplicationHealthStateHostedService : ApiApplicationHealthStateHostedService<ExampleDbContext, AppSettings>
{
    public ApplicationHealthStateHostedService(
        IApplicationHealthState applicationHealthState,
        IHostApplicationLifetime hostApplicationLifetime,
        IDbContextFactory<ExampleDbContext> dbContextFactory,
        IOptions<AppSettings> appSettings
    ) : base(
        applicationHealthState,
        hostApplicationLifetime,
        dbContextFactory,
        appSettings)
    {
    }

    protected override async Task InitDatabaseProduction(ExampleDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}