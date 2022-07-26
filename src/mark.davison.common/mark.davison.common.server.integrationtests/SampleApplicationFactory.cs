namespace mark.davison.common.server.integrationtests;

public class SampleApplicationFactory : WebApplicationFactory<Startup>, ICommonWebApplicationFactory<AppSettings>
{
    public Func<IRepository, Task> SeedDataFunc { get; set; } = _ => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(ConfigureServices);
        builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        });
    }
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
}
