namespace mark.davison.common.server.Services;

public abstract class ApiApplicationHealthStateHostedService<TDbContext, TAppSettings> : GenericApplicationHealthStateHostedService<TAppSettings>
    where TDbContext : DbContext
    where TAppSettings : class, IRootAppSettings
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    protected ApiApplicationHealthStateHostedService(
        IApplicationHealthState applicationHealthState,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<TAppSettings> appSettings,
        IDbContextFactory<TDbContext> dbContextFactory
    ) : base(
        applicationHealthState,
        hostApplicationLifetime,
        appSettings)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected abstract Task InitDatabaseProduction(TDbContext dbContext, CancellationToken cancellationToken);

    protected virtual async Task InitDatabaseDevelopment(TDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    protected override async Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            if (_appSettings.Value.PRODUCTION_MODE)
            {
                await InitDatabaseProduction(dbContext, cancellationToken);
            }
            else
            {
                await InitDatabaseDevelopment(dbContext, cancellationToken);
            }

            await AfterDbStartAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
            _applicationHealthState.ReadySource.SetException(e);
            throw;
        }

        _applicationHealthState.Ready = true;
        _applicationHealthState.ReadySource.SetResult();
    }

    protected Task AfterDbStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
