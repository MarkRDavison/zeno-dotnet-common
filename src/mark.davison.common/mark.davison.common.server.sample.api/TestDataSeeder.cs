namespace mark.davison.common.server.sample.api;

public interface ITestDataSeeder
{
    Task EnsureDataSeeded(CancellationToken cancellationToken);
}

public class TestDataSeeder : ITestDataSeeder
{
    protected readonly IRepository _repository;
    protected readonly AppSettings _appSettings;

    public TestDataSeeder(
        IRepository repository,
        IOptions<AppSettings> options
    )
    {
        _repository = repository;
        _appSettings = options.Value;
    }

    public async Task EnsureDataSeeded(CancellationToken cancellationToken)
    {
        await SeedData(_repository);
    }

    public Func<IRepository, Task> SeedData { get; set; } = _ => Task.CompletedTask;

}
