namespace mark.davison.common.client.Ignition;

public static class DependencyInjectionExtensions
{
    private static void AddSingleton<TAbstraction, TImplementation>(IServiceCollection services) where TAbstraction : class where TImplementation : class, TAbstraction
    {
        services.AddScoped<TAbstraction, TImplementation>();
    }

    // TODO: To common
    private static void InvokeRequestResponse(IServiceCollection services, MethodInfo methodInfo, Type genericType, Type type)
    {
        Type genericType2 = genericType;
        Type[] genericArguments = type.GetInterfaces().First((__) => __.IsGenericType && __.GetGenericTypeDefinition() == genericType2).GetGenericArguments();
        if (genericArguments.Length == 2)
        {
            Type type2 = genericArguments[0];
            Type type3 = genericArguments[1];
            Type type4 = genericType2.MakeGenericType(type2, type3);
            MethodInfo methodInfo2 = methodInfo.MakeGenericMethod(type4, type);
            object[] parameters = new IServiceCollection[1] { services };
            methodInfo2.Invoke(null, parameters);
        }
    }

    private static void InvokeAction(IServiceCollection services, MethodInfo methodInfo, Type genericType, Type type)
    {
        Type genericType2 = genericType;
        Type[] genericArguments = type.GetInterfaces().First((__) => __.IsGenericType && __.GetGenericTypeDefinition() == genericType2).GetGenericArguments();
        if (genericArguments.Length == 1)
        {
            Type type2 = genericArguments[0];
            Type type3 = genericType2.MakeGenericType(type2);
            MethodInfo methodInfo2 = methodInfo.MakeGenericMethod(type3, type);
            object[] parameters = new IServiceCollection[1] { services };
            methodInfo2.Invoke(null, parameters);
        }
    }

    public static IServiceCollection UseClientCQRS(this IServiceCollection services, params Type[] types)
    {
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IActionDispatcher, ActionDispatcher>();
        services.AddSingleton<ICQRSDispatcher, CQRSDispatcher>();
        var method = typeof(DependencyInjectionExtensions).GetMethod(nameof(AddSingleton), BindingFlags.Static | BindingFlags.NonPublic)!;


        Type commandHandlerType = typeof(ICommandHandler<,>);
        foreach (Type item in (from _ in types.SelectMany((_) => _.Assembly.ExportedTypes)
                               where _.GetInterfaces().Any((__) => __.IsGenericType && __.GetGenericTypeDefinition() == commandHandlerType)
                               select _).ToList())
        {
            InvokeRequestResponse(services, method, commandHandlerType, item);
        }

        Type queryHandlerType = typeof(IQueryHandler<,>);
        foreach (Type item2 in (from _ in types.SelectMany((_) => _.Assembly.ExportedTypes)
                                where _.GetInterfaces().Any((__) => __.IsGenericType && __.GetGenericTypeDefinition() == queryHandlerType)
                                select _).ToList())
        {
            InvokeRequestResponse(services, method, queryHandlerType, item2);
        }

        Type actionHandlerType = typeof(IActionHandler<>);
        foreach (Type item3 in (from _ in types.SelectMany((_) => _.Assembly.ExportedTypes)
                                where _.GetInterfaces().Any((__) => __.IsGenericType && __.GetGenericTypeDefinition() == actionHandlerType)
                                select _).ToList())
        {
            InvokeAction(services, method, actionHandlerType, item3);
        }

        return services;
    }
}
