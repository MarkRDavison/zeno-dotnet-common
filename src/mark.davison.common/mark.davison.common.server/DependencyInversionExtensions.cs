using mark.davison.common.server.Cron;

namespace mark.davison.common.server;

[ExcludeFromCodeCoverage]
public static class DependencyInversionExtensions
{

    private static void AddSingleton<TAbstraction, TImplementation>(IServiceCollection services)
    where TAbstraction : class
    where TImplementation : class, TAbstraction
    {
        services.AddScoped<TAbstraction, TImplementation>();
    }

    private static void InvokeRequestResponse(IServiceCollection services, MethodInfo methodInfo, Type genericType, Type type)
    {
        var interfaces = type.GetInterfaces();
        if (!interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == genericType))
        {
            return;
        }

        var interfaceType = interfaces.First(__ => __.IsGenericType && __.GetGenericTypeDefinition() == genericType);
        var genArgs = interfaceType.GetGenericArguments();
        if (genArgs.Length != 2)
        {
            return;
        }
        var requestType = genArgs[0];
        var responseType = genArgs[1];

        var abstraction = genericType.MakeGenericType(requestType, responseType);
        var implementation = type;

        var method = methodInfo.MakeGenericMethod(abstraction, implementation);
        method.Invoke(null, new[] { services });
    }

    // TODO: SourceGenerator
    public static IServiceCollection UseCQRS(this IServiceCollection services, params Type[] types)
    {
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        {
            var commandHandlerType = typeof(ICommandHandler<,>);

            var assemblyTypes = types
                .SelectMany(_ => _.Assembly.ExportedTypes)
                .Where(_ =>
                {
                    var interfaces = _.GetInterfaces();
                    return !_.IsGenericType && interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == commandHandlerType);
                })
                .ToList();

            var thisType = typeof(DependencyInversionExtensions);
            var methodInfo = thisType.GetMethod(nameof(DependencyInversionExtensions.AddSingleton), BindingFlags.NonPublic | BindingFlags.Static)!;

            foreach (var t in assemblyTypes)
            {
                InvokeRequestResponse(services, methodInfo, commandHandlerType, t);
            }
        }
        {
            var queryHandlerType = typeof(IQueryHandler<,>);

            var assemblyTypes = types
                .SelectMany(_ => _.Assembly.ExportedTypes)
                .Where(_ =>
                {
                    var interfaces = _.GetInterfaces();
                    return !_.IsGenericType && interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == queryHandlerType);
                })
                .ToList();

            var thisType = typeof(DependencyInversionExtensions);
            var methodInfo = thisType.GetMethod(nameof(DependencyInversionExtensions.AddSingleton), BindingFlags.NonPublic | BindingFlags.Static)!;

            foreach (var t in assemblyTypes)
            {
                InvokeRequestResponse(services, methodInfo, queryHandlerType, t);
            }
        }

        return services;
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