namespace mark.davison.common.server;

[ExcludeFromCodeCoverage]
public static class ProxyEndpointRouteBuilderExtensions
{
    // TODO: WebUtilities
    public static async Task<string> GetRequestBody(HttpRequest request)
    {
        try
        {
            if (request.ContentLength == 0) { return string.Empty; }
            var bodyStream = new StreamReader(request.Body);

            var bodyText = await bodyStream.ReadToEndAsync();
            return bodyText;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    // TODO: SourceGenerator
    private static async Task<object> PerformCommand<TRequest, TResponse>(HttpContext context, ICommandDispatcher dispatcher, CancellationToken cancellationToken)
        where TRequest : class, ICommand<TRequest, TResponse>, new()
        where TResponse : class, new()
    {
        var bodyText = await GetRequestBody(context.Request);
        if (string.IsNullOrEmpty(bodyText))
        {
            return new TResponse() as object;
        }
        var request = JsonSerializer.Deserialize<TRequest>(bodyText, SerializationHelpers.CreateStandardSerializationOptions());
        if (request == null)
        {
            return new TResponse() as object;
        }

        var responseObject = await dispatcher.Dispatch<TRequest, TResponse>(request, cancellationToken);
        return responseObject;
    }

    // TODO: SourceGenerator
    private static async Task<object> PerformQuery<TRequest, TResponse>(HttpContext context, IQueryDispatcher dispatcher, CancellationToken cancellationToken)
        where TRequest : class, IQuery<TRequest, TResponse>, new()
        where TResponse : class, new()
    {
        var requestProperties = typeof(TRequest)
            .GetProperties()
            .Where(_ => _.CanWrite)
            .ToList();

        var request = new TRequest();

        var queryDictionary = context.Request.Query.ToDictionary(_ => _.Key, _ => _.Value.ToString());
        foreach (var property in requestProperties.Where(_ => queryDictionary.ContainsKey(_.Name.ToLowerInvariant())))
        {
            var queryProperty = queryDictionary[property.Name.ToLowerInvariant()];

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(request, queryProperty);
            }
            else if (property.PropertyType == typeof(Guid))
            {
                if (Guid.TryParse(queryProperty, out var guid))
                {
                    property.SetValue(request, guid);
                }
                else
                {
                    throw new InvalidOperationException("Invalid Guid format");
                }
            }
            else if (property.PropertyType == typeof(long))
            {
                if (long.TryParse(queryProperty, out var lnum))
                {
                    property.SetValue(request, lnum);
                }
                else
                {
                    throw new InvalidOperationException("Invalid long format");
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                if (bool.TryParse(queryProperty, out var bval))
                {
                    property.SetValue(request, bval);
                }
                else
                {
                    throw new InvalidOperationException("Invalid bool format");
                }
            }
            else
            {
                throw new InvalidOperationException($"Unhandled get property type {property.PropertyType.Name}");
            }
        }

        var responseObject = await dispatcher.Dispatch<TRequest, TResponse>(request, cancellationToken);
        return responseObject;
    }

    // TODO: SourceGenerator
    public static IEndpointRouteBuilder ConfigureCQRSEndpoints(this IEndpointRouteBuilder endpoints, params Type[] types)
    {
        const string cqrsPath = "api";
        var commandInterfaceType = typeof(ICommand<,>);
        var queryInterfaceType = typeof(IQuery<,>);
        var assemblyTypes = types
            .SelectMany(_ => _.Assembly.ExportedTypes)
            .Where(_ => _.GetInterfaces().Any(__ => __.IsGenericType && (__.GetGenericTypeDefinition() == queryInterfaceType || __.GetGenericTypeDefinition() == commandInterfaceType)))
            .ToList();

        var thisType = typeof(ProxyEndpointRouteBuilderExtensions);
        var postMethodInfo = thisType.GetMethod(nameof(ProxyEndpointRouteBuilderExtensions.PerformCommand), BindingFlags.NonPublic | BindingFlags.Static)!;
        var getMethodInfo = thisType.GetMethod(nameof(ProxyEndpointRouteBuilderExtensions.PerformQuery), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (var commandType in assemblyTypes)
        {
            var getAttribute = commandType.CustomAttributes.FirstOrDefault(_ => _.AttributeType == typeof(GetRequestAttribute));
            if (getAttribute != null)
            {
                var path = getAttribute.NamedArguments.First(_ => _.MemberName == nameof(GetRequestAttribute.Path));
                var interfaceType = commandType.GetInterfaces().First(__ => __.IsGenericType && __.GetGenericTypeDefinition() == queryInterfaceType);
                var genArgs = interfaceType.GetGenericArguments();
                if (genArgs.Length != 2)
                {
                    continue;
                }
                var requestType = genArgs[0];
                var responseType = genArgs[1];

                var getMethod = getMethodInfo.MakeGenericMethod(requestType, responseType);
                // https://stackoverflow.com/questions/66793030/replacing-reflection-with-source-generators
                // https://bulldogjob.com/readme/source-generators-in-c-best-practices
                // Look at mvvm toolkit as an example
                endpoints.MapGet(
                    $"/{cqrsPath}/{path.TypedValue.Value as string}",
                    async (HttpContext context, IQueryDispatcher queryDispatcher, CancellationToken cancellationToken) =>
                    {
                        var result = getMethod.Invoke(null, new object[] { context, queryDispatcher, cancellationToken }) as Task<object>;
                        return await result!;
                    });
            }

            var postAttribute = commandType.CustomAttributes.FirstOrDefault(_ => _.AttributeType == typeof(PostRequestAttribute));
            if (postAttribute != null)
            {
                var path = postAttribute.NamedArguments.First(_ => _.MemberName == nameof(PostRequestAttribute.Path));

                var interfaceType = commandType.GetInterfaces().First(__ => __.IsGenericType && __.GetGenericTypeDefinition() == commandInterfaceType);
                var genArgs = interfaceType.GetGenericArguments();
                if (genArgs.Length != 2)
                {
                    continue;
                }
                var requestType = genArgs[0];
                var responseType = genArgs[1];

                var postMethod = postMethodInfo.MakeGenericMethod(requestType, responseType);
                // https://stackoverflow.com/questions/66793030/replacing-reflection-with-source-generators
                // Look at mvvm toolkit as an example
                endpoints.MapPost(
                    $"/{cqrsPath}/{path.TypedValue.Value as string}",
                    async (HttpContext context, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
                    {
                        var result = postMethod.Invoke(null, new object[] { context, commandDispatcher, cancellationToken }) as Task<object>;
                        return await result!;
                    });
            }
        }

        return endpoints;
    }
}
