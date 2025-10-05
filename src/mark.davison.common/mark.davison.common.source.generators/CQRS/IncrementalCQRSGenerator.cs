using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace mark.davison.common.source.generators.CQRS;

[Generator(LanguageNames.CSharp)]
public class IncrementalCQRSGenerator : IIncrementalGenerator
{
    public const string GeneratorNamespace = "mark.davison.common.source.generators.CQRS";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        context
            // .AddEmbeddedAttributeDefinition() TODO: Once in dotnet 10, need to add [global::Microsoft.CodeAnalysis.EmbeddedAttribute] to the attribute
            .RegisterPostInitializationOutput(_ =>
            {
                _.AddSource("UseCQRSServerAttribute.g.cs", SourceText.From(CQRSSources.UseCQRSServerAttribute(GeneratorNamespace), Encoding.UTF8));
                _.AddSource("UseCQRSClientAttribute.g.cs", SourceText.From(CQRSSources.UseCQRSClientAttribute(GeneratorNamespace), Encoding.UTF8));
            });

        context.SyntaxProvider
            .CreateSyntaxProvider<object?>(
                predicate: static (SyntaxNode s, CancellationToken token) => s is ClassDeclarationSyntax,
                transform: static (GeneratorSyntaxContext ctx, CancellationToken token) => {
                    return null;
                })
            .Where(static m => m is not null)
            .Collect();

        var current = context.SyntaxProvider
            .CreateSyntaxProvider<CqrsSourceGeneratorActivity?>(
                predicate: static (SyntaxNode s, CancellationToken token) => s is ClassDeclarationSyntax c,
                transform: static (GeneratorSyntaxContext ctx, CancellationToken token) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) is not INamedTypeSymbol symbol)
                    {
                        return default;
                    }

                    if (ParseMarkerAttribute(ctx, symbol) is { } attributeData)
                    {
                        return attributeData;
                    }

                    if (ParseRequestInterface(ctx, symbol) is { } requestData)
                    {
                        return requestData;
                    }

                    if (ParseProcessor(ctx, symbol) is { } processorData)
                    {
                        return processorData;
                    }

                    if (ParseValidator(ctx, symbol) is { } validatorData)
                    {
                        return validatorData;
                    }

                    if (ParseHandler(ctx, symbol) is { } handlerData)
                    {
                        return handlerData;
                    }

                    return null;
                })
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(current, static (spc, source) => Execute(source, spc));
    }

    private static CqrsSourceGeneratorActivity?ParseRequestInterface(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        if (symbol.AllInterfaces.Length == 0)
        {
            return null;
        }

        var commandInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.CQRS.ICommand`2")
                ?? throw new InvalidOperationException("Cannot find ICommand<TCommand, TResponse> type");

        var queryInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.CQRS.IQuery`2")
                ?? throw new InvalidOperationException("Cannot find IQuery<TCommand, TResponse> type");

        var postRequestAttributeType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.CQRS.PostRequestAttribute")
                ?? throw new InvalidOperationException("Cannot find PostRequestAttribute type");

        var getRequestAttributeType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.CQRS.GetRequestAttribute")
                ?? throw new InvalidOperationException("Cannot find GetRequestAttribute type");

        var definitionType = new List<Tuple<INamedTypeSymbol, CQRSActivityType, INamedTypeSymbol>>
            {
                new(commandInterfaceType, CQRSActivityType.Command, postRequestAttributeType),
                new(queryInterfaceType, CQRSActivityType.Query, getRequestAttributeType),
                // TODO: Action, ResponseAction
            };

        var attributes = symbol.GetAttributes();

        foreach (var (definitionSymbol, activity, routeAttribute) in definitionType)
        {
            foreach (var i in symbol.AllInterfaces)
            {
                if (!i.IsGenericType ||
                    i.TypeArguments.Length != 2)
                {
                    continue;
                }

                if (!SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, definitionSymbol))
                {
                    continue;
                }

                var requestType = i.TypeArguments[0];
                var responseType = i.TypeArguments[1];

                if (!SymbolEqualityComparer.Default.Equals(requestType, symbol))
                {
                    // TODO: Analyzer warning for invalid first parameter
                    continue;
                }

                string? endpoint = null;

                var endpointAttribute = attributes.FirstOrDefault(_ =>
                {
                    // TODO: Why doesn't this work
                    if (!SymbolEqualityComparer.Default.Equals(routeAttribute, _.AttributeClass))
                    {
                        return false;
                    }

                    if (_.NamedArguments.Any(na => na.Key == "Path"))
                    {
                        var pathArg = _.NamedArguments.Single(na => na.Key == "Path");

                        if (pathArg.Value.Value is string pathValue && !string.IsNullOrEmpty(pathValue)) 
                        {
                            endpoint = pathValue;
                        }
                    }


                    return true;
                });

                return new CqrsSourceGeneratorActivity(
                    true,
                    activity,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    "",
                    null,
                    null,
                    endpoint);
            }
        }

        return null;
    }

    private static CqrsSourceGeneratorActivity? ParseProcessor(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        if (symbol.AllInterfaces.Length == 0)
        {
            return null;
        }

        var commandProcessorInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.CQRS.Processors.ICommandProcessor`2")
                ?? throw new InvalidOperationException("Cannot find ICommandProcessor<TRequest, TResponse> type");

        var queryProcessorInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.CQRS.Processors.IQueryProcessor`2")
                ?? throw new InvalidOperationException("Cannot find IQueryProcessor<TRequest, TResponse> type");

        foreach (var i in symbol.AllInterfaces)
        {
            if (!i.IsGenericType || i.TypeArguments.Length != 2)
            {
                continue;
            }

            var requestType = i.TypeArguments[0];
            var responseType = i.TypeArguments[1];

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, commandProcessorInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Command,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    string.Empty,
                    null,
                    GetFullyQualifiedName(symbol),
                    null);
            }

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, queryProcessorInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Query,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    string.Empty,
                    null,
                    GetFullyQualifiedName(symbol),
                    null);
            }

        }

        return null;
    }

    private static CqrsSourceGeneratorActivity? ParseValidator(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        if (symbol.AllInterfaces.Length == 0)
        {
            return null;
        }

        var commandValidatorInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.CQRS.Validators.ICommandValidator`2")
                ?? throw new InvalidOperationException("Cannot find ICommandValidator<TRequest, TResponse> type");

        var queryValidatorInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.CQRS.Validators.IQueryValidator`2")
                ?? throw new InvalidOperationException("Cannot find IQueryValidator<TRequest, TResponse> type");

        foreach (var i in symbol.AllInterfaces)
        {
            if (!i.IsGenericType || i.TypeArguments.Length != 2)
            {
                continue;
            }

            var requestType = i.TypeArguments[0];
            var responseType = i.TypeArguments[1];

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, commandValidatorInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Command,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    string.Empty,
                    GetFullyQualifiedName(symbol),
                    null,
                    null);
            }

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, queryValidatorInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Query,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    string.Empty,
                    GetFullyQualifiedName(symbol),
                    null,
                    null);
            }

        }

        return null;
    }

    private static CqrsSourceGeneratorActivity? ParseHandler(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        if (symbol.AllInterfaces.Length == 0)
        {
            return null;
        }

        // TODO: Web vs server
        var commandHandlerInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.abstractions.CQRS.ICommandHandler`2")
                ?? throw new InvalidOperationException("Cannot find ICommandHandler<TCommand, TCommandResult> type");

        var queryHandlerInterfaceType = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.server.abstractions.CQRS.IQueryHandler`2")
                ?? throw new InvalidOperationException("Cannot find IQueryHandler<TQuery, TQueryResult> type");

        foreach (var i in symbol.AllInterfaces)
        {
            if (!i.IsGenericType || i.TypeArguments.Length != 2)
            {
                continue;
            }

            var requestType = i.TypeArguments[0];
            var responseType = i.TypeArguments[1];

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, commandHandlerInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Command,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    GetFullyQualifiedName(symbol),
                    null,
                    null,
                    null);
            }

            if (SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, queryHandlerInterfaceType))
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Query,
                    GetFullyQualifiedName(requestType),
                    GetFullyQualifiedName(responseType),
                    GetFullyQualifiedName(symbol),
                    null,
                    null,
                    null);
            }

        }

        return null;
    }

    private static CqrsSourceGeneratorActivity? ParseMarkerAttribute(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        var attributes = symbol.GetAttributes();

        if (attributes.Length > 0)
        {
            var markerAttributeType = ctx.SemanticModel.Compilation
                .GetTypeByMetadataName("mark.davison.common.source.generators.CQRS.UseCQRSServerAttribute")
                    ?? throw new InvalidOperationException("Cannot find UseCQRSServerAttribute type");

            AttributeData? markerAttribute = null;

            foreach (var attr in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, markerAttributeType))
                {
                    markerAttribute = attr;
                    break;
                }
            }

            if (markerAttribute is not null)
            {
                return new CqrsSourceGeneratorActivity(
                    false,
                    CQRSActivityType.Command,
                    "",
                    "",
                    "",
                    null,
                    null,
                    null);
            }

        }

        return null;
    }

    private static void Execute(ImmutableArray<CqrsSourceGeneratorActivity?> source, SourceProductionContext context)
    {

        var merged = source
            .OfType<CqrsSourceGeneratorActivity>()
            .GroupBy(_ => _.Key)
            .Select(MergeActivities)
            .ToImmutableArray();

        // CQRSDependecyInjectionExtensions.g.cs
        // CQRSEndpointRouteBuilderExtensions.g.cs

        context.AddSource($"CQRSServerDependecyInjectionExtensions.g.cs", CreateServerDependencyInjectionExtensions(merged));
    }

    private static void CreateServerDependencyRegistrationsForActivityType(
        ImmutableArray<CqrsSourceGeneratorActivity> source, 
        CQRSActivityType type,
        StringBuilder builder)
    {
        foreach (var activity in source.Where(_ => _.IsRequestDefinition && _.Type == type))
        {
            if (!string.IsNullOrEmpty(activity.Handler))
            {
                // If a handler is defined it takes priority.
                builder.AppendLine($"            services.AddScoped<I{type}Handler<{activity.Request},{activity.Response}>,{activity.Handler}>()");
            }
            else if (!string.IsNullOrEmpty(activity.Processor))
            {
                builder.AppendLine($"            services.AddScoped<I{type}Processor<{activity.Request},{activity.Response}>,{activity.Processor}>()");

                if (!string.IsNullOrEmpty(activity.Validator))
                {
                    builder.AppendLine($"            services.AddScoped<I{type}Validator<{activity.Request},{activity.Response}>,{activity.Validator}>()");
                }

                builder.AppendLine($"            services.AddScoped<I{type}Handler<{activity.Request},{activity.Response}>>(_ =>");
                builder.AppendLine($"            {{");
                builder.AppendLine($"                return new mark.davison.common.server.CQRS.ValidateAndProcess{type}Handler<{activity.Request},{activity.Response}>(");
                
                if (string.IsNullOrEmpty(activity.Validator))
                {
                    builder.AppendLine($"                    _.GetRequiredService<I{type}Processor<{activity.Request},{activity.Response}>()");
                }
                else
                {
                    builder.AppendLine($"                    _.GetRequiredService<I{type}Processor<{activity.Request},{activity.Response}>(),");
                    builder.AppendLine($"                    _.GetRequiredService<I{type}Validator<{activity.Request},{activity.Response}>()");
                }
                
                builder.AppendLine($"                );");
                builder.AppendLine($"            }});");
            }
            else
            {
                // No way to handle/process/validate.
                continue;
            }

            builder.AppendLine("");
        }
    }

    private static string CreateServerDependencyInjectionExtensions(ImmutableArray<CqrsSourceGeneratorActivity> source)
    {
        var builder = new StringBuilder();

        builder.AppendLine("using mark.davison.common.CQRS;");
        builder.AppendLine("using mark.davison.common.server.abstractions.CQRS;");
        builder.AppendLine("using mark.davison.common.server.Utilities;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using mark.davison.common.server.CQRS.Processors;");
        builder.AppendLine("using mark.davison.common.server.CQRS.Validators;");
        builder.AppendLine("");
        builder.AppendLine("namespace mark.davison.finance.api");
        builder.AppendLine("{");
        builder.AppendLine("");
        builder.AppendLine("    public static class CQRSDependecyInjectionExtensions");
        builder.AppendLine("    {");
        builder.AppendLine("");
        builder.AppendLine("        public static IServiceCollection AddCQRSServer(this IServiceCollection services)");
        builder.AppendLine("        {");
        builder.AppendLine("");
        builder.AppendLine("            services.AddScoped<mark.davison.common.server.abstractions.CQRS.IQueryDispatcher, mark.davison.common.server.CQRS.QueryDispatcher>();");
        builder.AppendLine("            services.AddScoped<mark.davison.common.server.abstractions.CQRS.ICommandDispatcher, mark.davison.common.server.CQRS.CommandDispatcher>();");
        builder.AppendLine("");

        CreateServerDependencyRegistrationsForActivityType(source, CQRSActivityType.Command, builder);
        CreateServerDependencyRegistrationsForActivityType(source, CQRSActivityType.Query, builder);

        builder.AppendLine("            return services;");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static CqrsSourceGeneratorActivity MergeActivities(IGrouping<string, CqrsSourceGeneratorActivity> grouping)
    {
        var root = grouping.First();

        foreach (var activity in grouping.Skip(1))
        {
            root = new CqrsSourceGeneratorActivity(
                true,
                root.Type is null ? activity.Type : root.Type,
                string.IsNullOrEmpty(root.Request) ? activity.Request : root.Request,
                string.IsNullOrEmpty(root.Response) ? activity.Response : root.Response,
                string.IsNullOrEmpty(root.Handler) ? activity.Handler : root.Handler,
                string.IsNullOrEmpty(root.Validator) ? activity.Validator : root.Validator,
                string.IsNullOrEmpty(root.Processor) ? activity.Processor : root.Processor,
                string.IsNullOrEmpty(root.Endpoint) ? activity.Endpoint : root.Endpoint);
        }

        return root;
    }

    public static string GetFullyQualifiedName(ITypeSymbol symbol)
    {
        return $"{GetNamespace(symbol)}.{symbol.Name}";
    }

    public static string GetNamespace(ITypeSymbol syntax)
    {
        string nameSpace = string.Empty;

        INamespaceSymbol? namespaceSymbol = syntax.ContainingNamespace;

        while (!string.IsNullOrEmpty(namespaceSymbol?.Name))
        {
            nameSpace = namespaceSymbol!.Name + (string.IsNullOrEmpty(nameSpace) ? "" : ".") + nameSpace;
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        return nameSpace;
    }
}
