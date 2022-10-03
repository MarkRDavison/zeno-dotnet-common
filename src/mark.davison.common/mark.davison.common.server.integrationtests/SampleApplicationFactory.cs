namespace mark.davison.common.server.integrationtests;

public class SampleApplicationFactory : WebApplicationFactory<Startup>, ICommonWebApplicationFactory<AppSettings>
{
    public Func<IRepository, Task> SeedDataFunc { get; set; } = _ => Task.CompletedTask;
    public IServiceProvider ServiceProvider => base.Services;

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
        var config = new AuthenticationConfig();
        config.SetBffBase("http://localhost/");
        services.AddLogging();
        services.AddHttpClient("API");
        services.AddSingleton<IAuthenticationConfig>(config);
        services.AddSingleton<IClientHttpRepository>(_ => new SampleClientHttpRepository(
            _.GetRequiredService<IAuthenticationConfig>().BffBase,
            CreateClient()));

        services.UseSampleApp(new()
        {
            API_ORIGIN = "http://localhost/"
        }, CreateClient);

        services.AddTransient<ITestDataSeeder, TestDataSeeder>(_ =>
            new TestDataSeeder(
                _.GetRequiredService<IRepository>(),
                _.GetRequiredService<IOptions<AppSettings>>()
            )
            {
                SeedData = SeedDataFunc
            });
    }
}
