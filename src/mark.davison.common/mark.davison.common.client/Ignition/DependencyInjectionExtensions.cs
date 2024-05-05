namespace mark.davison.common.client.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseCommonClient(
        this IServiceCollection services,
        IAuthenticationConfig authConfig,
        params Type[] types)
    {

        services
            .AddTransient(typeof(ModalViewModel<,>))
            .AddSingleton<IAuthenticationConfig>(authConfig)
            .AddSingleton<IAuthenticationContext, AuthenticationContext>()
            .AddSingleton<IDateService>(_ => new DateService(DateService.DateMode.Local))
            .AddSingleton<IClientNavigationManager, ClientNavigationManager>();

        // TODO: To source generator
        {
            var method = typeof(DependencyInjectionExtensions).GetMethod(nameof(AddTransient), BindingFlags.Static | BindingFlags.NonPublic)!;

            Type formSubmissionType = typeof(IFormSubmission<>);
            foreach (Type concreteType in (from _ in types.SelectMany((_) => _.Assembly.ExportedTypes)
                                           where _.GetInterfaces().Any((__) => __.IsGenericType && __.GetGenericTypeDefinition() == formSubmissionType)
                                           select _).ToList())
            {

                Type[] genericArguments = concreteType.GetInterfaces().First((__) => __.IsGenericType && __.GetGenericTypeDefinition() == formSubmissionType).GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    Type type = genericArguments[0];
                    Type interfaceType = formSubmissionType.MakeGenericType(type);
                    MethodInfo methodInfo2 = method.MakeGenericMethod(interfaceType, concreteType);
                    methodInfo2.Invoke(null, new[] { services });
                }

            }
        }

        return services;
    }

    public static IServiceCollection UseFluxorState(
        this IServiceCollection services,
        params Type[] types)
    {
        services
            .AddScoped<IStoreHelper, StoreHelper>()
            .AddFluxor(_ =>
            {
                foreach (var type in types)
                {
                    _.ScanAssemblies(type.Assembly);
                }
            });

        return services;
    }

    public static IServiceCollection UseClientRepository(this IServiceCollection services, string httpClientName, string localBffRoot)
    {
        services
            .AddSingleton<IClientHttpRepository>(_ =>
            {
                var context = _.GetRequiredService<IAuthenticationContext>();
                var authConfig = _.GetRequiredService<IAuthenticationConfig>();
                if (authConfig.BffBase == localBffRoot)
                {
                    authConfig.SetBffBase(string.Empty);
                }
                if (string.IsNullOrEmpty(authConfig.BffBase))
                {
                    var jsRuntime = _.GetRequiredService<IJSRuntime>();

                    if (jsRuntime is IJSInProcessRuntime jsInProcessRuntime)
                    {

                        string bffRoot = jsInProcessRuntime.Invoke<string>("GetBffUri", null);

                        if (string.IsNullOrEmpty(bffRoot))
                        {
                            bffRoot = localBffRoot;
                        }

                        authConfig.SetBffBase(bffRoot);
                    }
                }

                var clientHttpRepository = new ClientHttpRepository(
                        _.GetRequiredService<IAuthenticationConfig>().BffBase,
                        _.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName),
                        _.GetRequiredService<ILogger<ClientHttpRepository>>());

                clientHttpRepository.OnInvalidResponse += async (object? sender, HttpStatusCode status) =>
                {
                    if (status == HttpStatusCode.Unauthorized)
                    {
                        Console.Error.WriteLine("Received 401 - Validating auth state");
                        await context.ValidateAuthState();
                    }
                    else
                    {
                        Console.Error.WriteLine("Received HttpStatusCode.{0} - Not handling...", status);
                    }
                };

                return clientHttpRepository;
            })
            .AddHttpClient(httpClientName)
            .AddHttpMessageHandler(_ => new CookieHandler());
        return services;
    }

    private static void AddSingleton<TAbstraction, TImplementation>(IServiceCollection services)
        where TAbstraction : class
        where TImplementation : class, TAbstraction
    {
        services.AddScoped<TAbstraction, TImplementation>();
    }
    private static void AddTransient<TAbstraction, TImplementation>(IServiceCollection services)
        where TAbstraction : class
        where TImplementation : class, TAbstraction
    {
        services.AddTransient<TAbstraction, TImplementation>();
    }

    // TODO: To source generator
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
