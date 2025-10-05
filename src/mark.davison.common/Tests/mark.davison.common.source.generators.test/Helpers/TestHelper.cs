using System.Reflection;

namespace mark.davison.common.source.generators.test.Helpers;

public static class TestHelper
{
    public static GeneratorDriverRunResult RunSourceGenerator<TGenerator>(
        string source,
        IEnumerable<Type> typesToReference)
        where TGenerator : IIncrementalGenerator, new()
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        var assemblies = typesToReference
            .Select(_ => _.Assembly.Location)
            .Distinct()
            .Select(_ => MetadataReference.CreateFromFile(_))
            .ToList();

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            foreach (var a in entryAssembly.GetReferencedAssemblies())
            {
                assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location));
            }
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            references: [
                ..assemblies
            ],
            syntaxTrees: [syntaxTree]);

        var generator = new TGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult();
    }
}