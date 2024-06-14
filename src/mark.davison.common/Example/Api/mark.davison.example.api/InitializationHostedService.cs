namespace mark.davison.example.api;

public class InitializationHostedService : GenericApplicationHealthStateHostedService
{
    private readonly IDbContextFactory<ExampleDbContext> _dbContextFactory;
    private readonly IOptions<AppSettings> _appSettings;

    public InitializationHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IApplicationHealthState applicationHealthState,
        IOptions<AppSettings> appSettings,
        IDbContextFactory<ExampleDbContext> dbContextFactory
    ) : base(
        hostApplicationLifetime,
        applicationHealthState)
    {
        _appSettings = appSettings;
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (_appSettings.Value.PRODUCTION_MODE)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await base.AdditionalStartAsync(cancellationToken);
    }
}
