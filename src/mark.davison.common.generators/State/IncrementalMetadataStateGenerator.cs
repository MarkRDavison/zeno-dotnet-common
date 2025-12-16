using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mark.davison.common.generators.State;

[Generator(LanguageNames.CSharp)]
public class IncrementalMetadataStateGenerator : IIncrementalGenerator
{
    const string GeneratorNamespace = "mark.davison.common.client.state.generators";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var states = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (SyntaxNode s, CancellationToken token) => s is ClassDeclarationSyntax,
                transform: static (GeneratorSyntaxContext ctx, CancellationToken token) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) is not INamedTypeSymbol symbol)
                    {
                        return null;
                    }

                    var interfaces = symbol.Interfaces.Select(_ => _.Name).ToList();
                    var attributes = symbol.GetAttributes().Select(_ => _.AttributeClass?.Name).OfType<string>().ToList();

                    if (attributes.Any(_ =>
                        _.EndsWith("UseStateAttribute") ||
                        _.EndsWith("UseState")))
                    {
                        return new FeatureInformation(
                            symbol.Name,
                            SourceGeneratorHelpers.GetNamespace(symbol),
                            FeatureType.Registration,
                            [],
                            [],
                            []);
                    }

                    if (attributes.Any(_ =>
                        _.EndsWith("EffectAttribute") ||
                        _.EndsWith("Effect")))
                    {
                        var members = symbol.GetMembers();

                        if (attributes.Any(_ => _.EndsWith("EffectAttribute") || _.EndsWith("Effect")))
                        {
                            var effectMembers = members.Select(_ =>
                            {
                                if (_ is IMethodSymbol { IsStatic: false } methodSymbol)
                                {
                                    if (methodSymbol.ReturnType.Name != "Task" ||
                                        methodSymbol.Parameters.Length != 2 ||
                                        methodSymbol.Parameters[1].Type.Name != "IDispatcher")
                                    {
                                        return null;
                                    }

                                    return new EffectMethodInformation(
                                        SourceGeneratorHelpers.GetFullyQualifiedName(methodSymbol.Parameters[0].Type),
                                        methodSymbol.Name);
                                }

                                return null;
                            });

                            return new FeatureInformation(
                                symbol.Name,
                                SourceGeneratorHelpers.GetNamespace(symbol),
                                FeatureType.Effect,
                                [.. effectMembers.OfType<EffectMethodInformation>()],
                                [],
                                []);
                        }
                    }
                    {
                        var members = symbol.GetMembers();

                        var reducerMembers = members.Select(_ =>
                        {
                            var ats = _.GetAttributes().ToList();
                            var ats2 = _.GetAttributes().Select(a => a.AttributeClass?.Name).ToList();
                            var attributes = _.GetAttributes()
                                .Select(a => a.AttributeClass?.Name)
                                .Where(n =>
                                    n is not null &&
                                    (n.EndsWith("ReducerMethodAttribute") ||
                                    n.EndsWith("ReducerMethod")))
                                .ToList();

                            if (attributes.Any() && _ is IMethodSymbol { IsStatic: true, IsAsync: false } methodSymbol)
                            {
                                // TODO: Validate that param[1].Type implements BaseAction/Response??
                                // TODO: Return analyzer warning if param types etc dont match???

                                if (methodSymbol.Parameters.Length != 2 ||
                                    methodSymbol.Parameters[0].Type.Name != methodSymbol.ReturnType.Name)
                                {
                                    return null;
                                }

                                return new ReducerMethodInformation(
                                    SourceGeneratorHelpers.GetFullyQualifiedName(methodSymbol.ReturnType),
                                    SourceGeneratorHelpers.GetFullyQualifiedName(methodSymbol.Parameters[1].Type),
                                    methodSymbol.Name);
                            }

                            return null;
                        }).ToList();

                        if (symbol.IsStatic && reducerMembers.OfType<ReducerMethodInformation>().Any())
                        {
                            return new FeatureInformation(
                                symbol.Name,
                                SourceGeneratorHelpers.GetNamespace(symbol),
                                FeatureType.Reducer,
                                [],
                                [.. reducerMembers.OfType<ReducerMethodInformation>()],
                                []);
                        }
                    }

                    return null;
                })
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(states, static (spc, source) => Execute(source, spc));
    }

    private static void Execute(ImmutableArray<FeatureInformation?> sources, SourceProductionContext context)
    {
        if (!sources.Any(_ => _?.Type == FeatureType.Registration))
        {
            return;
        }

        var ignition = CreateIgnitionFile(GeneratorNamespace, sources);

        context.AddSource($"ClientStateDependecyInjectionExtensions.g.cs", ignition);
    }

    public static string CreateIgnitionFile(string assemblyMarkerClassNamespace, ImmutableArray<FeatureInformation?> sources)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"using Microsoft.Extensions.DependencyInjection;");
        stringBuilder.AppendLine($"using mark.davison.common.client.abstractions.Store;");
        stringBuilder.AppendLine($"using mark.davison.common.client.Store;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"// Generated at: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"namespace {assemblyMarkerClassNamespace}");
        stringBuilder.AppendLine($"{{");
        stringBuilder.AppendLine($"    public static class ClientStateDependecyInjectionExtensions");
        stringBuilder.AppendLine($"    {{");
        stringBuilder.AppendLine($"        public static IServiceCollection AddClientState(this IServiceCollection services)");
        stringBuilder.AppendLine($"        {{");
        stringBuilder.AppendLine($"            services.AddSingleton(typeof(IState<>), typeof(StateImplementation<>));");
        stringBuilder.AppendLine($"            services.AddSingleton<IStoreHelper, StoreHelper>();");
        stringBuilder.AppendLine($"            services.AddSingleton<IDispatcher, Dispatcher>();");
        stringBuilder.AppendLine($"            services.AddSingleton<IActionSubscriber, ActionSubscriber>();");
        stringBuilder.AppendLine();

        // TODO: update when common-ified
        const string StateStoreFullyQualified = "mark.davison.common.client.Store.StateStore";

        // effect types
        foreach (var s in sources.OfType<FeatureInformation>().Where(_ => _.Type == FeatureType.Effect))
        {
            stringBuilder.AppendLine($"            services.AddTransient<{s.Namespace}.{s.Name}>();");

            foreach (var em in s.EffectMethods)
            {
                var str = "            " +
                    $"{StateStoreFullyQualified}.RegisterEffectCallback" +
                    $"<{em.ActionType}, {s.Namespace}.{s.Name}>" +
                    $"((services, action, dispatcher) => services.GetRequiredService" +
                    $"<{s.Namespace}.{s.Name}>()." +
                    $"{em.MemberName}(action, dispatcher));";

                stringBuilder.AppendLine(str);
            }

            if (s.EffectMethods.Length > 0)
            {
                stringBuilder.AppendLine();
            }
        }

        // reducer types
        foreach (var s in sources.OfType<FeatureInformation>().Where(_ => _.Type == FeatureType.Reducer))
        {
            foreach (var rm in s.ReducerMethods)
            {
                stringBuilder.AppendLine($"            {StateStoreFullyQualified}.RegisterReducerCallback<{rm.Action}, {rm.StateType}>((state, action) => {s.Namespace}.{s.Name}.{rm.MemberName}(state, action));");
            }

            if (s.ReducerMethods.Length > 0)
            {
                stringBuilder.AppendLine();
            }
        }

        stringBuilder.AppendLine($"            return services;");
        stringBuilder.AppendLine($"        }}");
        stringBuilder.AppendLine($"    }}");
        stringBuilder.AppendLine($"}}");

        return stringBuilder.ToString();
    }
}
