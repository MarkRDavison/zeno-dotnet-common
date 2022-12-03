namespace mark.davison.common.server.Health;

public class GenericApplicationHealthStateHostedService : IApplicationHealthStateHostedService
{
    protected readonly IHostApplicationLifetime _hostApplicationLifetime;
    protected readonly IApplicationHealthState _applicationHealthState;

    public GenericApplicationHealthStateHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IApplicationHealthState applicationHealthState)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _applicationHealthState = applicationHealthState;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            _applicationHealthState.Started = true;
        });

        _hostApplicationLifetime.ApplicationStopping.Register(() =>
        {
            _applicationHealthState.Ready = false;
        });

        _hostApplicationLifetime.ApplicationStopped.Register(() =>
        {
            _applicationHealthState.Ready = false;
        });

        _ = AdditionalStartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await AdditionalStopAsync(cancellationToken);
        _applicationHealthState.Ready = false;
    }

    protected virtual Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        _applicationHealthState.Ready = true;
        return Task.CompletedTask;
    }

    protected virtual Task AdditionalStopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}