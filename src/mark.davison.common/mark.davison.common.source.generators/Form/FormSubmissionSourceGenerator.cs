using mark.davison.common.source.generators.CQRS;
using System;

namespace mark.davison.common.source.generators.Form;

[Generator(LanguageNames.CSharp)]
public class FormSubmissionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(_ =>
        {
            _.AddSource("UseFormAttribute.g.cs", SourceText.From(FormSources.UseFormAttribute("mark.davison.common.source.generators.client"), Encoding.UTF8));
        });

        var sources = context.SyntaxProvider
            .CreateSyntaxProvider<FormSubmissionInfo?>(
                predicate: static (SyntaxNode s, CancellationToken token) => s is ClassDeclarationSyntax,
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

                    if (ParseFormSubmissionInterface(ctx, symbol) is { } formSubmissionInterface)
                    {
                        return formSubmissionInterface;
                    }

                    return null;
                })
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(sources, static (spc, source) => Execute(source, spc));
    }

    private static FormSubmissionInfo? ParseMarkerAttribute(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        var attributes = symbol.GetAttributes();

        if (attributes.Length > 0)
        {
            var markerAttributeType = ctx.SemanticModel.Compilation
                .GetTypeByMetadataName("mark.davison.common.source.generators.client.UseFormAttribute")
                    ?? throw new InvalidOperationException("Cannot find UseFormAttribute type");

            AttributeData? markerAttribute = null;

            foreach (var attr in attributes)
            {
                // TODO: Doesn't work ?
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, markerAttributeType))
                {
                    markerAttribute = attr;
                    break;
                }

                // TODO: This is a fallback
                if (attr.AttributeClass?.Name == markerAttributeType.Name)
                {
                    markerAttribute = attr;
                    break;
                }
            }

            if (markerAttribute?.AttributeClass is not null)
            {
                return new FormSubmissionInfo(
                    string.Empty, 
                    string.Empty, 
                    string.Empty, 
                    SourceGeneratorHelpers.GetNamespace(symbol));
            }

        }

        return null;
    }

    private static void Execute(ImmutableArray<FormSubmissionInfo?> source, SourceProductionContext context)
    {
        var rootNamespace = source.FirstOrDefault(_ => !string.IsNullOrEmpty(_.RootNamespace))?.RootNamespace;

        if (string.IsNullOrEmpty(rootNamespace))
        {
            return;
        }

        StringBuilder builder = new();
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();
        builder.AppendLine($"namespace {rootNamespace}");
        builder.AppendLine($"{{");
        builder.AppendLine($"    public static class FormSubmissionDependencyInjectionExtensions");
        builder.AppendLine($"    {{");
        builder.AppendLine($"        public static IServiceCollection AddForms(this IServiceCollection services)");
        builder.AppendLine($"        {{");

        foreach (var submissionInfo in source.Where(_ => string.IsNullOrEmpty(_.RootNamespace)))
        {
            builder.AppendLine($"            services.AddTransient<{submissionInfo.FormSubmissionInterface},{submissionInfo.FormSubmissionImplementation}>();");
        }

        builder.AppendLine($"            return services;");
        builder.AppendLine($"        }}");
        builder.AppendLine($"    }}");
        builder.AppendLine($"}}");

        context.AddSource(
            "FormSubmissionDependecyInjectionExtensions.g.cs",
            builder.ToString());
    }

    private static FormSubmissionInfo? ParseFormSubmissionInterface(GeneratorSyntaxContext ctx, INamedTypeSymbol symbol)
    {
        if (symbol.AllInterfaces.Length == 0)
        {
            return null;
        }

        var formSubmissionInterface = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("mark.davison.common.client.abstractions.Form.IFormSubmission`1");

        if (formSubmissionInterface is null) 
        {
            return null;
        }

        foreach (var i in symbol.AllInterfaces)
        {
            if (!i.IsGenericType ||
                i.TypeArguments.Length != 1)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, formSubmissionInterface))
            {
                continue;
            }

            var viewModelType = i.TypeArguments[0];

            return new FormSubmissionInfo(
                SourceGeneratorHelpers.GetFullyQualifiedName(i),
                SourceGeneratorHelpers.GetFullyQualifiedName(symbol),
                SourceGeneratorHelpers.GetFullyQualifiedName(viewModelType),
                string.Empty);
        }

        return null;
    }
}
