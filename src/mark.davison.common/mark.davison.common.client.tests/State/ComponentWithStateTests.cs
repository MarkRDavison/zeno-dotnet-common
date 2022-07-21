namespace mark.davison.common.client.tests.State;

[TestClass]
public class ComponentWithStateTests
{

    private Mock<IStateStore> _stateStore = default!;
    private Mock<IComponentSubscriptions> _componentSubscriptions = default!;
    private TestComponent _component = default!;

    [TestInitialize]
    public void TestInitialise()
    {
        _stateStore = new(MockBehavior.Strict);
        _componentSubscriptions = new(MockBehavior.Strict);
        _component = new TestComponent
        {
            StateStore = _stateStore.Object,
            ComponentSubscriptions = _componentSubscriptions.Object
        };
    }

    [TestMethod]
    public void GetState_AddsComponentSubscription()
    {
        _componentSubscriptions
            .Setup(_ => _.Add<TestState>(_component))
            .Verifiable();

        _stateStore
            .Setup(_ => _.GetState<TestState>())
            .Returns(new StateInstance<TestState>(() => new TestState()));

        var state = _component.GetState<TestState>();

        _componentSubscriptions
            .Verify(_ => _.Add<TestState>(_component),
                Times.Once);
    }

    [TestMethod]
    public void GetState_GetsStateFromStateStore()
    {
        _componentSubscriptions
            .Setup(_ => _.Add<TestState>(_component));

        _stateStore
            .Setup(_ => _.GetState<TestState>())
            .Returns(new StateInstance<TestState>(() => new TestState()))
            .Verifiable();

        var state = _component.GetState<TestState>();

        _stateStore
            .Verify(_ => _.GetState<TestState>(),
                Times.Once);
    }

    [TestMethod]
    public void DisposingComponent_RemovesFromComponentSubscriptions()
    {
        _componentSubscriptions
            .Setup(_ => _.Remove(_component))
            .Verifiable();

        _component.Dispose();

        _componentSubscriptions
            .Verify(_ => _.Remove(_component),
                Times.Once);
    }

    [TestMethod]
    public void SetState_InvokesStateStore()
    {
        var state = new TestState();

        _stateStore
            .Setup(_ => _.SetState<TestState>(state))
            .Verifiable();

        _component.SetState(state);

        _stateStore
            .Verify(_ => _.SetState<TestState>(state),
                Times.Once);
    }
}
