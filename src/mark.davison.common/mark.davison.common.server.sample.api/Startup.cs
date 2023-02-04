using mark.davison.common.server.Configuration;
using mark.davison.common.server.Endpoints;

namespace mark.davison.common.server.sample.api;

public class Startup
{
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

        services
            .AddControllers();

        services
            .ConfigureHealthCheckServices<InitializationHostedService>();

        services
            .AddHttpClient()
            .AddHttpContextAccessor();

        services.UseCQRS();
        services.UseLegacyCQRS(typeof(Startup));
        services.UseLegacyCQRSValidatorsAndProcessors(typeof(Startup));
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
                .ConfigureLegacyCQRSEndpoints(typeof(Startup));
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

    public IConfiguration Configuration { get; }
    public AppSettings AppSettings { get; set; } = null!;
}
