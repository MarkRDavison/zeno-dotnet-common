namespace mark.davison.common.server;

[ExcludeFromCodeCoverage]
public static class DependencyInversionExtensions
{
    public static IServiceCollection AddZenoAuthentication(this IServiceCollection services, Action<ZenoAuthOptions> setupAction)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        services.AddOptions();
        services.Configure(setupAction);
        services.AddHttpClient(ZenoAuthenticationConstants.ProxyHttpClientName);
        services.AddHttpClient(ZenoAuthenticationConstants.AuthClientName);
        services.AddTransient<IZenoAuthenticationSession, ZenoAuthenticationSession>();
        services.AddHostedService<OpenIdConnectResolver>();

        return services;
    }

    public static void ConfigureHealthCheckServices(this IServiceCollection services)
    {
        services.ConfigureHealthCheckServices<GenericApplicationHealthStateHostedService>();
    }

    public static void ConfigureHealthCheckServices<THealthHosted>(this IServiceCollection services)
        where THealthHosted : class, IApplicationHealthStateHostedService
    {
        services.AddSingleton<IApplicationHealthState, ApplicationHealthState>();

        services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(StartupHealthCheck.Name)
            .AddCheck<LiveHealthCheck>(LiveHealthCheck.Name)
            .AddCheck<ReadyHealthCheck>(ReadyHealthCheck.Name);

        services.AddHostedService<THealthHosted>();
    }

    public static void MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapHealthChecks("/health/startup", new HealthCheckOptions
            {
                Predicate = r => r.Name == StartupHealthCheck.Name
            })
            .AllowAnonymous();
        endpoints
            .MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name == LiveHealthCheck.Name
            })
            .AllowAnonymous();
        endpoints
            .MapHealthChecks("/health/readiness", new HealthCheckOptions
            {
                Predicate = r => r.Name == ReadyHealthCheck.Name
            })
            .AllowAnonymous();
    }

    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
    {
        var config = new ScheduleConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
        }

        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }

}