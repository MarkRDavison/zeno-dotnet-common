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

        await Assert.That(sourceString).Contains("public partial class ExampleStateComponent");
        await Assert.That(sourceString).Contains("protected override void SubscribeToStateChanges()");
        await Assert.That(sourceString).Contains("ExampleStateInstance.OnStateChange += OnExampleStateChange;");
        await Assert.That(sourceString).Contains("AnotherStateInstance.OnStateChange += OnAnotherStateChange;");
        await Assert.That(sourceString).Contains("ExampleStateInstance.OnStateChange -= OnExampleStateChange;");
        await Assert.That(sourceString).Contains("AnotherStateInstance.OnStateChange -= OnAnotherStateChange;");
        await Assert.That(sourceString).Contains("private void OnExampleStateChange(object? sender, EventArgs args)");
        await Assert.That(sourceString).Contains("private void OnAnotherStateChange(object? sender, EventArgs args)");
        await Assert.That(sourceString).Contains("public required IState<ExampleState> ExampleStateInstance { get; set; }");
        await Assert.That(sourceString).Contains("public required IState<AnotherState> AnotherStateInstance { get; set; }");
        await Assert.That(sourceString).Contains("public ExampleState ExampleState => ExampleStateInstance.Value;");
        await Assert.That(sourceString).Contains("public AnotherState AnotherState => AnotherStateInstance.Value;");
    }
}