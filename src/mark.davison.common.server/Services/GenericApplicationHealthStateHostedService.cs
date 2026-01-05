namespace mark.davison.common.server.Services;

public abstract class GenericApplicationHealthStateHostedService<TAppSettings> : IHostedService
    where TAppSettings : class, IRootAppSettings
{
    protected readonly IApplicationHealthState _applicationHealthState;
    protected readonly IHostApplicationLifetime _hostApplicationLifetime;
    protected readonly IOptions<TAppSettings> _appSettings;

    public GenericApplicationHealthStateHostedService(
        IApplicationHealthState applicationHealthState,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<TAppSettings> appSettings)
    {
        _applicationHealthState = applicationHealthState;
        _hostApplicationLifetime = hostApplicationLifetime;
        _appSettings = appSettings;
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

        if (_appSettings.Value.PRODUCTION_MODE)
        {
            _ = AdditionalStartAsync(cancellationToken);
        }
        else
        {
            try
            {
                await AdditionalStartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                throw;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _applicationHealthState.Ready = false;
    }

    protected virtual async Task AdditionalStartAsync(CancellationToken cancellationToken)
    {
        _applicationHealthState.Ready = true;
        _applicationHealthState.ReadySource.SetResult();
    }
}
