namespace mark.davison.common.client;

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

    private static void InvokeAction(IServiceCollection services, MethodInfo methodInfo, Type genericType, Type type)
    {
        var interfaceType = type.GetInterfaces().First(__ => __.IsGenericType && __.GetGenericTypeDefinition() == genericType);
        var genArgs = interfaceType.GetGenericArguments();
        if (genArgs.Length != 1)
        {
            return;
        }
        var actionType = genArgs[0];

        var abstraction = genericType.MakeGenericType(actionType);
        var implementation = type;

        var method = methodInfo.MakeGenericMethod(abstraction, implementation);
        method.Invoke(null, new[] { services });
    }

    public static IServiceCollection UseState(this IServiceCollection services)
    {
        services.AddSingleton<IStateStore, StateStore>();
        services.AddSingleton<IComponentSubscriptions, ComponentSubscriptions>();
        return services;
    }

    // TODO: Source Generator
    public static IServiceCollection UseCQRS(this IServiceCollection services, params Type[] types)
    {
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IActionDispatcher, ActionDispatcher>();
        services.AddSingleton<ICQRSDispatcher, CQRSDispatcher>();

        var thisType = typeof(DependencyInversionExtensions);
        var methodInfo = thisType.GetMethod(nameof(DependencyInversionExtensions.AddSingleton), BindingFlags.NonPublic | BindingFlags.Static)!;


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

            foreach (var t in assemblyTypes)
            {
                InvokeRequestResponse(services, methodInfo, queryHandlerType, t);
            }
        }
        {
            var actionHandlerType = typeof(IActionHandler<>);
            var assemblyTypes = types
                .SelectMany(_ => _.Assembly.ExportedTypes)
                .Where(_ =>
                {
                    var interfaces = _.GetInterfaces();
                    return !_.IsGenericType && interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == actionHandlerType);
                })
                .ToList();

            foreach (var t in assemblyTypes)
            {
                InvokeAction(services, methodInfo, actionHandlerType, t);
            }
        }
        {
            var actionHandlerType = typeof(IResponseActionHandler<,>);
            var assemblyTypes = types
                .SelectMany(_ => _.Assembly.ExportedTypes)
                .Where(_ =>
                {
                    var interfaces = _.GetInterfaces();
                    return !_.IsGenericType && interfaces.Any(__ => __.IsGenericType && __.GetGenericTypeDefinition() == actionHandlerType);
                })
                .ToList();

            foreach (var t in assemblyTypes)
            {
                InvokeRequestResponse(services, methodInfo, actionHandlerType, t);
            }
        }

        return services;
    }
}
