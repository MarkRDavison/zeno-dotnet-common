namespace mark.davison.common.server.sample.api;

public interface ITestDataSeeder
{
    Task EnsureDataSeeded(CancellationToken cancellationToken);
}

public class TestDataSeeder : ITestDataSeeder
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly AppSettings _appSettings;

    public TestDataSeeder(
        IServiceProvider serviceProvider,
        IOptions<AppSettings> options
    )
    {
        _serviceProvider = serviceProvider;
        _appSettings = options.Value;
    }

    public async Task EnsureDataSeeded(CancellationToken cancellationToken)
    {
        await SeedData(_serviceProvider);
    }

    public Func<IServiceProvider, Task> SeedData { get; set; } = _ => Task.CompletedTask;

}
