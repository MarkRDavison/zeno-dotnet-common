namespace mark.davison.common.client.tests.State;

[TestClass]
public class ComponentSubscriptionsTests
{
    private readonly ComponentSubscriptions _componentSubscriptions;

    public ComponentSubscriptionsTests()
    {
        _componentSubscriptions = new();
    }

    [TestMethod]
    public void ReRenderSubscribersByType_InvokesReRender_OnAllComponentsRegisteredForStateType()
    {
        List<Mock<IComponentWithState>> componentMocks = new()
        {
            new(MockBehavior.Strict), new(MockBehavior.Strict), new(MockBehavior.Strict)
        };

        foreach (var component in componentMocks)
        {
            component.Setup(_ => _.ReRender()).Verifiable();
            _componentSubscriptions.Add<TestState>(component.Object);
        }

        _componentSubscriptions.ReRenderSubscribers(typeof(TestState));

        foreach (var component in componentMocks)
        {
            component.Verify(_ => _.ReRender(), Times.Once);
        }
    }

    [TestMethod]
    public void ReRenderSubscribersByGeneric_InvokesReRender_OnAllComponentsRegisteredForStateType()
    {
        List<Mock<IComponentWithState>> componentMocks = new()
        {
            new(MockBehavior.Strict), new(MockBehavior.Strict), new(MockBehavior.Strict)
        };

        foreach (var component in componentMocks)
        {
            component.Setup(_ => _.ReRender()).Verifiable();
            _componentSubscriptions.Add<TestState>(component.Object);
        }

        _componentSubscriptions.ReRenderSubscribers<TestState>();

        foreach (var component in componentMocks)
        {
            component.Verify(_ => _.ReRender(), Times.Once);
        }
    }

    [TestMethod]
    public void Remove_StopsReRenderCalls()
    {
        List<Mock<IComponentWithState>> componentMocks = new()
        {
            new(MockBehavior.Strict), new(MockBehavior.Strict), new(MockBehavior.Strict)
        };

        foreach (var component in componentMocks)
        {
            component.Setup(_ => _.ReRender()).Verifiable();
            _componentSubscriptions.Add<TestState>(component.Object);
            _componentSubscriptions.Remove(component.Object);
        }

        _componentSubscriptions.ReRenderSubscribers<TestState>();

        foreach (var component in componentMocks)
        {
            component.Verify(_ => _.ReRender(), Times.Never);
        }
    }

}
