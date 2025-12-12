using mark.davison.common.client.abstractions.Store;
using mark.davison.common.client.web;

namespace mark.davison.common.generators.tests.State;

public sealed class IncrementalStateComponentGeneratorTests
{
    [Test]
    public async Task TestComponentStateGeneration()
    {
        var source = @"
using mark.davison.common.client.web;
using mark.davison.common.client.abstractions;
using mark.davison.common.client.abstractions.Store;

namespace mark.davison.generators.tests
{
    public class ExampleState : IClientState
    {
    }
    public class AnotherState : IClientState
    {
    }

    [StatePropertyAttribute<ExampleState>]
    [StatePropertyAttribute<AnotherState>]
    public partial class ExampleStateComponent : StateComponent
    {
    }
}
";

        var result = GeneratorTestHelpers.RunSourceGenerator<IncrementalStateComponentGenerator>(
            source,
            [
                typeof(Object),
                typeof(GCSettings),
                typeof(SourceGeneratorHelpers),
                typeof(StateComponent),
                typeof(StatePropertyAttribute<>),
                typeof(IClientState)
            ]);

        await Assert.That(result.Results).HasSingleItem();

        var componentSource = result.Results
            .First()
            .GeneratedSources
            .FirstOrDefault(_ => _.HintName == "ExampleStateComponent_StateComponent.g.cs");

        var sourceString = componentSource.SourceText.ToString();
    }
}
