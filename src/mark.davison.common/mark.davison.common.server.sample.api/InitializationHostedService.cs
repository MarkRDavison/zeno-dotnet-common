namespace mark.davison.common.server.sample.api;

public class InitializationHostedService : GenericApplicationHealthStateHostedService
{
    private readonly ITestDataSeeder _dataSeeder;

    public InitializationHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IApplicationHealthState applicationHealthState,
        ITestDataSeeder dataSeeder
    ) : base(
        hostApplicationLifetime,
        applicationHealthState
    )
    {
        _dataSeeder = dataSeeder;
    }

    protected override async Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        await _dataSeeder.EnsureDataSeeded(cancellationToken);
        await base.AdditionalStartAsync(cancellationToken);
    }

}
