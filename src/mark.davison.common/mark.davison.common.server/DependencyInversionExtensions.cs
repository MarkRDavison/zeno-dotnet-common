namespace mark.davison.common.server;

[ExcludeFromCodeCoverage]
public static class DependencyInversionExtensions
{
    public static void UseRedis(this IServiceCollection services,
        RedisAppSettings redisSettings,
        string appName,
        bool productionMode)
    {

        if (string.IsNullOrEmpty(redisSettings.PASSWORD) ||
            string.IsNullOrEmpty(redisSettings.HOST))
        {
            services
                .AddDistributedMemoryCache();
        }
        else
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { redisSettings.HOST + ":" + redisSettings.PORT },
                Password = redisSettings.PASSWORD
            };
            IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(config);
            services.AddStackExchangeRedisCache(_ =>
            {
                _.InstanceName = appName.Replace('-', '_') + "_" + (productionMode ? "prod_" : "dev_");
                _.Configuration = redis.Configuration;
            });
            services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtectionKeys");
            services.AddSingleton(redis);
        }
    }

    public static void UseRedisSession(this IServiceCollection services,
        AuthAppSettings authSettings,
        RedisAppSettings redisSettings,
        string appName,
        bool productionMode)
    {
        services
            .AddSession(o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.Name = authSettings.SESSION_NAME;
                o.Cookie.HttpOnly = true;
            });

        services
            .UseRedis(redisSettings, appName, productionMode);
    }

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