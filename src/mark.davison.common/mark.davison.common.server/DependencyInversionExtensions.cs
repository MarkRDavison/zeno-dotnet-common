namespace mark.davison.common.server;

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
        var interfaceType = type.GetInterfaces().First(__ => __.IsGenericType && __.GetGenericTypeDefinition() == genericType);
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
                    return interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == commandHandlerType);
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
                    return interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == queryHandlerType);
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

}