namespace mark.davison.common.server.integrationtests;

public class SampleApplicationFactory : WebApplicationFactory<Startup>, ICommonWebApplicationFactory<AppSettings>
{
    public Func<IServiceProvider, Task> SeedDataFunc { get; set; } = _ => Task.CompletedTask;
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
        services.AddSingleton<IEntityDefaulter<Author>, AuthorDefaulter>();
        services.AddSingleton<IAuthenticationConfig>(config);
        services.AddSingleton<IClientHttpRepository>(_ => new SampleClientHttpRepository(
            _.GetRequiredService<IAuthenticationConfig>().BffBase,
            CreateClient(),
            _.GetRequiredService<ILogger<SampleClientHttpRepository>>()));

        services.UseSampleApp(new()
        {
            API_ORIGIN = "http://localhost/"
        }, CreateClient);

    }
}
