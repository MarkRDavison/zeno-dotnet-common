namespace mark.davison.common.generators;

[ExcludeFromCodeCoverage]
public static class SourceGeneratorHelpers
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;
        }

        foreach (var childNs in ns.GetNamespaceMembers())
        {
            foreach (var type in childNs.GetAllTypes())
            {
                yield return type;
            }
        }
    }

    public static string GetFullyQualifiedName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string GetNamespace(ITypeSymbol syntax)
    {
        string nameSpace = string.Empty;

        INamespaceSymbol? namespaceSymbol = syntax.ContainingNamespace;

        while (!string.IsNullOrEmpty(namespaceSymbol?.Name))
        {
            nameSpace = namespaceSymbol!.Name + (string.IsNullOrEmpty(nameSpace) ? string.Empty : ".") + nameSpace;
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        return nameSpace;
    }
}
