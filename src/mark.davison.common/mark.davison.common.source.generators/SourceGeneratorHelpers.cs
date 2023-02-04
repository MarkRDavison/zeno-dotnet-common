﻿namespace mark.davison.common.source.generators;

[ExcludeFromCodeCoverage]
public static class SourceGeneratorHelpers
{
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

    public static IEnumerable<ITypeSymbol> GetAllTypes(INamespaceSymbol root)
    {
        foreach (var namespaceOrTypeSymbol in root.GetMembers())
        {
            if (namespaceOrTypeSymbol is INamespaceSymbol ns)
            {
                foreach (var nested in GetAllTypes(ns))
                {
                    yield return nested;
                }
            }

            else if (namespaceOrTypeSymbol is ITypeSymbol type)
            {
                yield return type;
            }
        }
    }

    public static List<ITypeSymbol> GetPotentialTypeSymbols(GeneratorExecutionContext context, HashSet<string> namespaces)
    {
        return context.Compilation.SourceModule.ReferencedAssemblySymbols
            .Where(_ => namespaces.Any(__ => _.Identity.Name.StartsWith(__)))
            .SelectMany(_ =>
            {
                try
                {
                    var main = _.Identity.Name.Split('.').Aggregate(_.GlobalNamespace, (s, c) => s.GetNamespaceMembers().Single(m => m.Name.Equals(c)));

                    return GetAllTypes(main);
                }
                catch
                {
                    return Enumerable.Empty<ITypeSymbol>();
                }
            })
            .ToList();
    }
}
