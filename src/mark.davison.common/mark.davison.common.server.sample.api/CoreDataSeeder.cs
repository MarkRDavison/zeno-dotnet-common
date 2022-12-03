namespace mark.davison.common.server.sample.api;

public interface ICoreDataSeeder
{
    Task EnsureDataSeeded(CancellationToken cancellationToken);
}

public class CoreDataSeeder : ICoreDataSeeder
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IApplicationHealthState _applicationHealthState;
    protected readonly AppSettings _appSettings;

    public CoreDataSeeder(
        IServiceProvider serviceProvider,
        IApplicationHealthState applicationHealthState,
        IOptions<AppSettings> options
    )
    {
        _serviceProvider = serviceProvider;
        _applicationHealthState = applicationHealthState;
        _appSettings = options.Value;
    }

    public async Task EnsureDataSeeded(CancellationToken cancellationToken)
    {
        await SeedData(_serviceProvider);
    }

    public Func<IServiceProvider, Task> SeedData { get; set; } = _ => Task.CompletedTask;

}
