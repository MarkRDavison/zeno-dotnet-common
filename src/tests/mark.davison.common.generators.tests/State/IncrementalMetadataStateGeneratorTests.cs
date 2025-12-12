using mark.davison.common.client.abstractions.Store;

namespace mark.davison.common.generators.tests.State;

public sealed class IncrementalMetadataStateGeneratorTests
{
    [Test]
    public async Task TestStateGeneration()
    {
        var source = @"
using System.Threading;
using System.Threading.Tasks;
using mark.davison.common.client.abstractions.Store;

namespace mark.davison.generator.tests
{

    [UseState]
    public class MarkerClass
    {
    
    }

    public class SomeState : IClientState
    {

    }

    public sealed class SomeEffectAction1;
    public sealed class SomeEffectActionResponse1;

    [Effect]
    public sealed class SomeStateEffects
    {
        public async Task HandleSomeStateEffectMethod1(SomeEffectAction1 action, IDispatcher dispatcher)
        {
            await Task.CompletedTask;
        }
    }

    public static class SomeStateReducer
    {
        [Reducer]
        public static SomeState HandleSomeEffectActionResponse1(SomeState state, SomeEffectActionResponse1 response)
        {
            return state;
        }
    }

}
";

        var result = GeneratorTestHelpers.RunSourceGenerator<IncrementalMetadataStateGenerator>(
            source,
            [
                typeof(Object),
                typeof(GCSettings),
                typeof(SourceGeneratorHelpers),
                typeof(IDispatcher),
                typeof(FeatureType),
                typeof(IClientState),
                typeof(UseStateAttribute),
                typeof(EffectAttribute),
                typeof(ReducerMethodAttribute)
            ]);

        await Assert.That(result.Results).HasSingleItem();

        var ignitionsource = result.Results
            .First()
            .GeneratedSources
            .FirstOrDefault(_ => _.HintName == "ClientStateDependecyInjectionExtensions.g.cs");

        var sourceString = ignitionsource.SourceText.ToString();

        await Assert.That(sourceString).IsNotNullOrEmpty();

        await Assert.That(sourceString).Contains("public static class ClientStateDependecyInjectionExtensions");
        await Assert.That(sourceString).Contains("services.AddSingleton(typeof(IState<>), typeof(StateImplementation<>));");
        await Assert.That(sourceString).Contains("services.AddTransient<mark.davison.generator.tests.SomeStateEffects>();\r\n");
        await Assert.That(sourceString).Contains("mark.davison.common.client.Store.StateStore.RegisterEffectCallback<global::mark.davison.generator.tests.SomeEffectAction1, mark.davison.generator.tests.SomeStateEffects>((services, action, dispatcher) => services.GetRequiredService<mark.davison.generator.tests.SomeStateEffects>().HandleSomeStateEffectMethod1(action, dispatcher));");
        await Assert.That(sourceString).Contains("mark.davison.common.client.Store.StateStore.RegisterReducerCallback<global::mark.davison.generator.tests.SomeEffectActionResponse1, global::mark.davison.generator.tests.SomeState>((state, action) => mark.davison.generator.tests.SomeStateReducer.HandleSomeEffectActionResponse1(state, action));");
    }
}
