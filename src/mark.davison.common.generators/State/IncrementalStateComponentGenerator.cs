using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mark.davison.common.generators.State;

[Generator(LanguageNames.CSharp)]
public class IncrementalStateComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var info = context.SyntaxProvider
            .CreateSyntaxProvider<StateComponentInformation?>(
                predicate: SyntaxPredicate,
                transform: static (GeneratorSyntaxContext ctx, CancellationToken token) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) is not INamedTypeSymbol symbol)
                    {
                        return null;
                    }

                    var baseClasses = new List<INamedTypeSymbol>();
                    {
                        var current = symbol.BaseType;

                        while (current is not null)
                        {
                            baseClasses.Add(current);
                            current = current.BaseType;
                        }
                    }

                    if (!baseClasses.Any(b => string.Equals(b.ToDisplayString(), "mark.davison.common.client.web.StateComponent")))
                    {
                        return null;
                    }

                    var info = new StateComponentInformation
                    {
                        ComponentName = symbol.Name,
                        ComponentNamespace = SourceGeneratorHelpers.GetNamespace(symbol)
                    };

                    foreach (var att in symbol.GetAttributes())
                    {
                        var name = att.AttributeClass?.ToDisplayString();

                        if (string.IsNullOrEmpty(name) ||
                            !att.AttributeClass!.IsGenericType ||
                            att.AttributeClass.ConstructedFrom.TypeParameters.Length is not 1 ||
                            !name!.StartsWith("mark.davison.common.client.abstractions.Store.StatePropertyAttribute<"))
                        {
                            continue;
                        }

                        var attrSyntax = att.ApplicationSyntaxReference?.GetSyntax(token) as AttributeSyntax;

                        if (attrSyntax is null)
                        {
                            continue;
                        }

                        if (attrSyntax.Name is GenericNameSyntax genericName &&
                            genericName.TypeArgumentList.Arguments.Count == 1)
                        {
                            var typeSyntax = genericName.TypeArgumentList.Arguments.First();


                            if (ctx.SemanticModel.GetTypeInfo(typeSyntax, token).Type is INamedTypeSymbol typeSymbol)
                            {
                                if (info.States.All(_ => _.Name != typeSymbol.Name))
                                {
                                    info.States.Add(new StateForComponentInformation
                                    {
                                        Name = typeSymbol.Name,
                                        Namespace = SourceGeneratorHelpers.GetNamespace(typeSymbol)
                                    });
                                }
                            }
                        }

                    }

                    return info;
                }
        )
        .Where(static m => m is not null)
        .Collect();

        context.RegisterSourceOutput(info, static (spc, source) => Execute(source, spc));
    }

    private static void Execute(ImmutableArray<StateComponentInformation?> sources, SourceProductionContext context)
    {
        foreach (var source in sources.OfType<StateComponentInformation>())
        {
            var definition = CreateComponentDefinition(source);
            context.AddSource($"{source.ComponentName}_StateComponent.g.cs", definition);
        }
    }

    private static string CreateComponentDefinition(StateComponentInformation source)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"#nullable enable");
        builder.AppendLine();
        builder.AppendLine($"using Microsoft.AspNetCore.Components;");
        builder.AppendLine($"using mark.davison.common.client.abstractions.Store;");
        builder.AppendLine($"using {source.ComponentNamespace};");
        foreach (var state in source.States)
        {
            builder.AppendLine($"using {state.Namespace};");
        }
        builder.AppendLine($"");
        builder.AppendLine($"namespace {source.ComponentNamespace};");
        builder.AppendLine($"");
        builder.AppendLine($"public partial class {source.ComponentName}");
        builder.AppendLine($"{{");
        builder.AppendLine($"    protected override void SubscribeToStateChanges()");
        builder.AppendLine($"    {{");
        foreach (var state in source.States)
        {
            builder.AppendLine($"        {state.Name}Instance.OnStateChange += On{state.Name}Change;");
        }
        builder.AppendLine($"    }}");
        builder.AppendLine($"");
        builder.AppendLine($"    protected override void UnsubscribeToStateChanges()");
        builder.AppendLine($"    {{");
        foreach (var state in source.States)
        {
            builder.AppendLine($"        {state.Name}Instance.OnStateChange -= On{state.Name}Change;");
        }
        builder.AppendLine($"    }}");
        builder.AppendLine($"");
        foreach (var state in source.States)
        {
            builder.AppendLine($"    private void On{state.Name}Change(object? sender, EventArgs args)");
            builder.AppendLine($"    {{");
            builder.AppendLine($"        _ = UpdateState();");
            builder.AppendLine($"    }}");
            builder.AppendLine($"    ");
        }
        foreach (var state in source.States)
        {
            builder.AppendLine($"    [Inject]");
            builder.AppendLine($"    public required IState<{state.Name}> {state.Name}Instance {{ get; set; }}");
            builder.AppendLine($"    ");
        }
        foreach (var state in source.States)
        {
            builder.AppendLine($"    public {state.Name} {state.Name} => {state.Name}Instance.Value;");
            builder.AppendLine($"    ");
        }

        builder.AppendLine($"}}");

        return builder.ToString();
    }

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken token)
    {
        if (s is not ClassDeclarationSyntax cds)
        {
            return false;
        }

        var name = cds.Identifier;

        return cds.AttributeLists
                  .SelectMany(al => al.Attributes)
                  .Any(a => a.Name.ToString()
                  .Contains("StateProperty"));
    }
}
