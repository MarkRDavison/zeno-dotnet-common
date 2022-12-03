namespace mark.davison.common.server.sample.api;

public class InitializationHostedService : GenericApplicationHealthStateHostedService
{
    private readonly ICoreDataSeeder _coreDataSeeder;

    public InitializationHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IApplicationHealthState applicationHealthState,
        ICoreDataSeeder coreDataSeeder
    ) : base(
        hostApplicationLifetime,
        applicationHealthState
    )
    {
        _coreDataSeeder = coreDataSeeder;
    }

    protected override async Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        await _coreDataSeeder.EnsureDataSeeded(cancellationToken);
        await base.AdditionalStartAsync(cancellationToken);
    }

}
