namespace mark.davison.common.source.generators.CQRS;

[ExcludeFromCodeCoverage]
[Generator]
public class CQRSGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(_ =>
        {
            _.AddSource("UseCQRSServerAttribute.g.cs", SourceText.From(CQRSSources.UseCQRSServerAttribute("mark.davison.common.source.generators.CQRS"), Encoding.UTF8));
            _.AddSource("UseCQRSClientAttribute.g.cs", SourceText.From(CQRSSources.UseCQRSClientAttribute("mark.davison.common.source.generators.CQRS"), Encoding.UTF8));
        });
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var (assemblyMarkerClassNamespace, namespaces, type) = FetchCQRSNamespaces(context);

        if (!namespaces.Any())
        {
            return;
        }

        var symbols = SourceGeneratorHelpers.GetPotentialTypeSymbols(context, namespaces);

        List<CQRSActivity> activities = new();
 
        foreach (var symbol in symbols)
        {
            var commandActivity = AttemptCreateCommand(symbol, symbols);
            if (commandActivity != null)
            {
                activities.Add(commandActivity);
            }

            var queryActivity = AttemptCreateQuery(symbol, symbols);
            if (queryActivity != null)
            {
                activities.Add(queryActivity);
            }

            var actionActivity = AttemptCreateAction(symbol, symbols);
            if (actionActivity != null)
            {
                activities.Add(actionActivity);
            }

            var responseActionActivity = AttemptCreateResponseAction(symbol, symbols);
            if (responseActionActivity != null)
            {
                activities.Add(responseActionActivity);
            }
        }

        var diNamespaces = new HashSet<string>
        {
            "Microsoft.Extensions.DependencyInjection",
            "mark.davison.common.CQRS"
        };

        if (type == CQRSType.Server)
        {
            diNamespaces.Add("mark.davison.common.server.Utilities");
        }


        GenerateDepdendencyInjectionExtensions(context, assemblyMarkerClassNamespace, diNamespaces, activities, type);
        if (type == CQRSType.Server)
        {
            diNamespaces.Add("Microsoft.AspNetCore.Builder");
            GenerateEndpointRouteExtensions(context, assemblyMarkerClassNamespace, diNamespaces, activities);
        }
    }

    private CQRSActivity? AttemptCreateCommand(ITypeSymbol symbol, List<ITypeSymbol> symbols)
    {
        var commandInterface = symbol.AllInterfaces.FirstOrDefault(_ => _.Name.Equals("ICommand") && _.TypeArguments.Length == 2);
        var commandHandler = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("ICommandHandler") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == commandInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == commandInterface?.TypeArguments[1]?.Name);
        });
        var processor = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("ICommandProcessor") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == commandInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == commandInterface?.TypeArguments[1]?.Name);
        }); 
        var validator = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("ICommandValidator") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == commandInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == commandInterface?.TypeArguments[1]?.Name);
        });
        if (commandInterface != null && commandHandler != null)
        {
            var requestFQN = commandInterface.TypeArguments[0].ToDisplayString();
            var responseFQN = commandInterface.TypeArguments[1].ToDisplayString();

            var requestAttribute = symbol.GetAttributes().FirstOrDefault(_ => _.AttributeClass?.Name?.EndsWith("PostRequestAttribute") ?? false);

            return new CQRSActivity
            {
                Type = CQRSActivityType.Command,
                Request = requestFQN,
                Response = responseFQN,
                Handler = commandHandler.ToDisplayString(),
                Processor = processor?.ToDisplayString() ?? string.Empty,
                Validator = validator?.ToDisplayString() ?? string.Empty,
                Path = requestAttribute?.NamedArguments.First(_ => _.Key == "Path").Value.Value as string ?? string.Empty
            };
        }

        return null;
    }

    private CQRSActivity? AttemptCreateQuery(ITypeSymbol symbol, List<ITypeSymbol> symbols)
    {
        var queryInterface = symbol.AllInterfaces.FirstOrDefault(_ => _.Name.Equals("IQuery") && _.TypeArguments.Length == 2);
        var queryHandler = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("IQueryHandler") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == queryInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == queryInterface?.TypeArguments[1]?.Name);
        });
        var processor = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("IQueryProcessor") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == queryInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == queryInterface?.TypeArguments[1]?.Name);
        });
        var validator = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("IQueryValidator") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == queryInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == queryInterface?.TypeArguments[1]?.Name);
        });
        if (queryInterface != null && queryHandler != null)
        {
            var requestFQN = queryInterface.TypeArguments[0].ToDisplayString();
            var responseFQN = queryInterface.TypeArguments[1].ToDisplayString();

            var requestAttribute = symbol.GetAttributes().FirstOrDefault(_ => _.AttributeClass?.Name?.EndsWith("GetRequestAttribute") ?? false);

            return new CQRSActivity
            {
                Type = CQRSActivityType.Query,
                Request = requestFQN,
                Response = responseFQN,
                Handler = queryHandler.ToDisplayString(),
                Processor = processor?.ToDisplayString() ?? string.Empty,
                Validator = validator?.ToDisplayString() ?? string.Empty,
                Path = requestAttribute?.NamedArguments.First(_ => _.Key == "Path").Value.Value as string ?? string.Empty
            };
        }
        return null;
    }

    private CQRSActivity? AttemptCreateAction(ITypeSymbol symbol, List<ITypeSymbol> symbols)
    {
        var actionInterface = symbol.AllInterfaces.FirstOrDefault(_ => _.Name.Equals("IAction") && _.TypeArguments.Length == 1);
        var actionHandler = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("IActionHandler") &&
                _.TypeArguments.Length == 1 &&
                _.TypeArguments[0].Name == actionInterface?.TypeArguments[0]?.Name);
        });

        if (actionInterface != null && actionHandler != null)
        {
            var requestFQN = actionInterface.TypeArguments[0].ToDisplayString();

            return new CQRSActivity
            {
                Type = CQRSActivityType.Action,
                Request = requestFQN,
                Handler = actionHandler.ToDisplayString()
            };
        }

        return null;
    }

    private CQRSActivity? AttemptCreateResponseAction(ITypeSymbol symbol, List<ITypeSymbol> symbols)
    {
        var actionInterface = symbol.AllInterfaces.FirstOrDefault(_ => _.Name.Equals("IResponseAction") && _.TypeArguments.Length == 2);
        var actionHandler = symbols.FirstOrDefault(_ =>
        {
            return null != _.AllInterfaces.FirstOrDefault(_ =>
                _.Name.Equals("IResponseActionHandler") &&
                _.TypeArguments.Length == 2 &&
                _.TypeArguments[0].Name == actionInterface?.TypeArguments[0]?.Name &&
                _.TypeArguments[1].Name == actionInterface?.TypeArguments[1]?.Name);
        });

        if (actionInterface != null && actionHandler != null)
        {
            var requestFQN = actionInterface.TypeArguments[0].ToDisplayString();
            var responseFQN = actionInterface.TypeArguments[1].ToDisplayString();

            return new CQRSActivity
            {
                Type = CQRSActivityType.ResponseAction,
                Request = requestFQN,
                Response = responseFQN,
                Handler = actionHandler.ToDisplayString()
            };
        }

        return null;
    }

    private (string, HashSet<string>, CQRSType) FetchCQRSNamespaces(GeneratorExecutionContext context)
    {
        CQRSType cqrsType = CQRSType.Server;
        var types = context.Compilation.SourceModule.GlobalNamespace
            .GetNamespaceMembers()
            .SelectMany(SourceGeneratorHelpers.GetAllTypes)
            .ToList();

        var assemblyMarkerClass = types
            .FirstOrDefault(_ => _
                .GetAttributes()
                .Any(__ => __.AttributeClass?.Name == "UseCQRSServerAttribute"));

        if (assemblyMarkerClass == null)
        {
            cqrsType = CQRSType.Client;
            assemblyMarkerClass = types
            .FirstOrDefault(_ => _
                .GetAttributes()
                .Any(__ => __.AttributeClass?.Name == "UseCQRSClientAttribute"));
        }

        if (assemblyMarkerClass == null)
        {
            return (string.Empty, new(), cqrsType);
        }

        var at = assemblyMarkerClass.GetAttributes().FirstOrDefault(_ => _.AttributeClass?.Name == (cqrsType == CQRSType.Server ? "UseCQRSServerAttribute" : "UseCQRSClientAttribute"));

        var assemblyMarkerClassNamespace = SourceGeneratorHelpers.GetNamespace(assemblyMarkerClass);

        var args = at?.ConstructorArguments.FirstOrDefault();
        if (args == null)
        {
            return (string.Empty, new(), cqrsType);
        }

        var cqrsTypes = args.Value.Values.Select(_ => _.Value as INamedTypeSymbol).Where(_ => _ != null).Cast<INamedTypeSymbol>().ToList();
        return (assemblyMarkerClassNamespace, new(cqrsTypes.Select(SourceGeneratorHelpers.GetNamespace)), cqrsType);
    }

    private static void GenerateEndpointRouteExtensions(GeneratorExecutionContext context, string @namespace, HashSet<string> namespaces, List<CQRSActivity> activities)
    {
        StringBuilder stringBuilder = new();
        foreach (var n in namespaces.OrderBy(_ => _))
        {
            stringBuilder.AppendLine($"using {n};");
        }

        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine($"namespace {@namespace}");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("    public static class GenerateEndpointRouteExtensions");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("        public static void ConfigureCQRSEndpoints(this IEndpointRouteBuilder endpoints)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine(string.Empty);

        foreach (var activity in activities.Where(_ => !string.IsNullOrEmpty(_.Path)))
        {
            if (activity.Type == CQRSActivityType.Command)
            {
                AppendPost(stringBuilder, activity);

            }
            else if (activity.Type == CQRSActivityType.Query)
            {
                AppendGet(stringBuilder, activity);
            }
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("}");

        context.AddSource("CQRSEndpointRouteBuilderExtensions.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
    }

    private static void AppendGet(StringBuilder stringBuilder, CQRSActivity activity)
    {
        stringBuilder.AppendLine("            endpoints.MapGet(");
        stringBuilder.AppendLine($"                \"/api/{activity.Path}\",");
        stringBuilder.AppendLine("                async (HttpContext context, CancellationToken cancellationToken) =>");
        stringBuilder.AppendLine("                {");
        stringBuilder.AppendLine("                    var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();");
        stringBuilder.AppendLine($"                    var request = WebUtilities.GetRequestFromQuery<{activity.Request},{activity.Response}>(context.Request);");
        stringBuilder.AppendLine($"                    return await dispatcher.Dispatch<{activity.Request},{activity.Response}>(request, cancellationToken);");
        stringBuilder.AppendLine("                });");
        stringBuilder.AppendLine(string.Empty);
    }

    private static void AppendPost(StringBuilder stringBuilder, CQRSActivity activity)
    {
        stringBuilder.AppendLine("            endpoints.MapPost(");
        stringBuilder.AppendLine($"                \"/api/{activity.Path}\",");
        stringBuilder.AppendLine("                async (HttpContext context, CancellationToken cancellationToken) =>");
        stringBuilder.AppendLine("                {");
        stringBuilder.AppendLine("                    var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();");
        stringBuilder.AppendLine($"                    var request = await WebUtilities.GetRequestFromBody<{activity.Request},{activity.Response}>(context.Request);");
        stringBuilder.AppendLine($"                    return await dispatcher.Dispatch<{activity.Request},{activity.Response}>(request, cancellationToken);");
        stringBuilder.AppendLine("                });");
        stringBuilder.AppendLine(string.Empty);
    }

    private static void GenerateDepdendencyInjectionExtensions(GeneratorExecutionContext context, string @namespace, HashSet<string> diNamespaces, List<CQRSActivity> activities, CQRSType type)
    {
        StringBuilder stringBuilder = new();
        foreach (var n in diNamespaces.OrderBy(_ => _))
        {
            stringBuilder.AppendLine($"using {n};");
        }

        if (type == CQRSType.Server)
        {
            stringBuilder.AppendLine($"using mark.davison.common.server.CQRS.Processors;");
            stringBuilder.AppendLine($"using mark.davison.common.server.CQRS.Validators;");
        }

        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine($"namespace {@namespace}");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("    public static class CQRSDependecyInjectionExtensions");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine(string.Empty);

        if (type == CQRSType.Server)
        {
            stringBuilder.AppendLine("        public static void UseCQRSServer(this IServiceCollection services)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.server.abstractions.CQRS.IQueryDispatcher, mark.davison.common.server.CQRS.QueryDispatcher>();");
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.server.abstractions.CQRS.ICommandDispatcher, mark.davison.common.server.CQRS.CommandDispatcher>();");
        }
        else
        {
            stringBuilder.AppendLine("        public static void UseCQRSClient(this IServiceCollection services)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.client.abstractions.CQRS.IQueryDispatcher, mark.davison.common.client.CQRS.QueryDispatcher>();");
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.client.abstractions.CQRS.ICommandDispatcher, mark.davison.common.client.CQRS.CommandDispatcher>();");
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.client.abstractions.CQRS.IActionDispatcher, mark.davison.common.client.CQRS.ActionDispatcher>();");
            stringBuilder.AppendLine("            services.AddScoped<mark.davison.common.client.abstractions.CQRS.ICQRSDispatcher, mark.davison.common.client.CQRS.CQRSDispatcher>();");
        }

        stringBuilder.AppendLine(string.Empty);

        foreach (var command in activities.Where(_ => _.Type == CQRSActivityType.Command))
        {
            stringBuilder.AppendLine($"            services.AddScoped<ICommandHandler<{command.Request},{command.Response}>, {command.Handler}>();");
            if (!string.IsNullOrEmpty(command.Validator))
            {
                stringBuilder.AppendLine($"            services.AddScoped<ICommandValidator<{command.Request},{command.Response}>, {command.Validator}>();");
            }
            if (!string.IsNullOrEmpty(command.Processor))
            {
                stringBuilder.AppendLine($"            services.AddScoped<ICommandProcessor<{command.Request},{command.Response}>, {command.Processor}>();");
            }
        }
        foreach (var query in activities.Where(_ => _.Type == CQRSActivityType.Query))
        {
            stringBuilder.AppendLine($"            services.AddScoped<IQueryHandler<{query.Request},{query.Response}>, {query.Handler}>();");
            if (!string.IsNullOrEmpty(query.Validator))
            {
                stringBuilder.AppendLine($"            services.AddScoped<IQueryValidator<{query.Request},{query.Response}>, {query.Validator}>();");
            }
            if (!string.IsNullOrEmpty(query.Processor))
            {
                stringBuilder.AppendLine($"            services.AddScoped<IQueryProcessor<{query.Request},{query.Response}>, {query.Processor}>();");
            }
        }
        foreach (var action in activities.Where(_ => _.Type == CQRSActivityType.ResponseAction))
        {
            stringBuilder.AppendLine($"            services.AddScoped<IResponseActionHandler<{action.Request},{action.Response}>, {action.Handler}>();");
        }
        foreach (var action in activities.Where(_ => _.Type == CQRSActivityType.Action))
        {
            stringBuilder.AppendLine($"            services.AddScoped<IActionHandler<{action.Request}>, {action.Handler}>();");
        }

        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("}");

        context.AddSource("CQRSDependecyInjectionExtensions.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
    }
}
