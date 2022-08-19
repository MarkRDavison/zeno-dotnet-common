﻿namespace mark.davison.common.server.sample.api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }


    public void ConfigureServices(IServiceCollection services)
    {
        var configured = Configuration.GetSection(AppSettings.SECTION);

        services.Configure<AppSettings>(configured);
        AppSettings = new AppSettings();
        configured.Bind(AppSettings);
        if (AppSettings == null) { throw new InvalidOperationException(); }

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader());
        });

        services
            .AddControllers();

        services
            .ConfigureHealthCheckServices();

        services
            .AddHttpClient()
            .AddHttpContextAccessor();

        services.UseCQRS(typeof(Startup));
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
                .ConfigureCQRSEndpoints(typeof(Startup));
        });
    }

    public IConfiguration Configuration { get; }
    public AppSettings AppSettings { get; set; } = null!;
}
