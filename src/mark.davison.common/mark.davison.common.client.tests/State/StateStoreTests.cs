namespace mark.davison.common.client.tests.State;

[TestClass]
public class StateStoreTests
{
    private readonly Mock<IComponentSubscriptions> _componentSubscriptions = new(MockBehavior.Strict);
    private readonly StateStore _stateStore;

    public StateStoreTests()
    {
        _stateStore = new(_componentSubscriptions.Object);
    }

    [TestMethod]
    public void SetState_InvokesReRenderSubscribers()
    {
        _componentSubscriptions
            .Setup(_ => _.ReRenderSubscribers(typeof(TestState)))
            .Verifiable();

        var state = new TestState();

        _stateStore.SetState(state);

        _componentSubscriptions
            .Verify(_ => _.ReRenderSubscribers(typeof(TestState)), Times.Once);
    }

    [TestMethod]
    public void Reset_InvokesInitialise_OnRegisteredState()
    {
        _componentSubscriptions
            .Setup(_ => _.ReRenderSubscribers(typeof(TestState)));

        var state = new TestState();

        bool initialised = false;
        state.OnInitialise = () =>
        {
            initialised = true;
        };

        _stateStore.SetState(state);

        _stateStore.Reset();

        Assert.IsTrue(initialised);

    }

    [TestMethod]
    public void Reset_InvokesReRenderSubscribers_OnRegisteredState()
    {
        _componentSubscriptions
            .Setup(_ => _.ReRenderSubscribers(typeof(TestState)))
            .Verifiable();

        var state = new TestState();

        _stateStore.SetState(state);

        _componentSubscriptions.Invocations.Clear();

        _stateStore.Reset();

        _componentSubscriptions
            .Verify(_ => _.ReRenderSubscribers(typeof(TestState)), Times.Once);
    }

    [TestMethod]
    public void GetState_WhereStateIsNotRegistered_InitialisesNewStateInstance()
    {
        var stateInstance = _stateStore.GetState<TestState>();

        Assert.AreEqual(1, stateInstance.Instance.InitCount);

        stateInstance = _stateStore.GetState<TestState>();

        Assert.AreEqual(1, stateInstance.Instance.InitCount);
    }

    [TestMethod]
    public void GetState_WhereStateIsRegistered_ReturnsExistingState()
    {
        _componentSubscriptions
            .Setup(_ => _.ReRenderSubscribers(typeof(TestState)));

        var state = new TestState()
        {
            InitCount = 542,
        };

        _stateStore.SetState(state);

        var stateInstance = _stateStore.GetState<TestState>();

        Assert.AreEqual(state.InitCount, stateInstance.Instance.InitCount);
    }

}
