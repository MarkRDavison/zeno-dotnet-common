namespace mark.davison.common.server.sample.api;

public class Startup
{
    public IConfiguration Configuration { get; }
    public AppSettings AppSettings { get; set; } = null!;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        AppSettings = services.ConfigureSettingsServices<AppSettings>(Configuration);
        if (AppSettings == null) { throw new InvalidOperationException(); }

        services.AddCors(options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                builder => builder
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader());
        });

        services.AddTransient<ICoreDataSeeder, CoreDataSeeder>(_ =>
            new CoreDataSeeder(
                _.GetRequiredService<IServiceProvider>(),
                _.GetRequiredService<IApplicationHealthState>(),
                _.GetRequiredService<IOptions<AppSettings>>()
            ));

        services
            .AddControllers();

        services
            .ConfigureHealthCheckServices<InitializationHostedService>();

        var dbSettings = new DatabaseAppSettings
        {
            DATABASE_TYPE = DatabaseType.Sqlite,
            CONNECTION_STRING = "RANDOM"
        };

        services
            .AddHttpClient()
            .AddHttpContextAccessor()
            .UseDatabase<TestDbContext>(false, dbSettings)
            .UseCoreDbContext<TestDbContext>(false, dbSettings);

        services.UseCQRSServer();
        services
            .AddSingleton<IChangesetQueue, RepositoryBackedChangesetQueue>()
            .AddTransient<IChangesetGroup, ChangesetGroup>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCors("CorsPolicy");
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints
                .MapHealthChecks();
            endpoints
                .MapControllers();
            endpoints
                .ConfigureCQRSEndpoints();

            endpoints.UseGet<Comment>();
            endpoints.UseGetById<Comment>();
            endpoints.UsePost<Comment>();
            endpoints.UseDelete<Comment>();

            endpoints.UseGet<Author>();
            endpoints.UseGetById<Author>();
            endpoints.UsePost<Author>();
            endpoints.UseDelete<Author>();
        });
    }
}
