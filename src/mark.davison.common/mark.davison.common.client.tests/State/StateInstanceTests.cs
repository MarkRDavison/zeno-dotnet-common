namespace mark.davison.common.client.tests.State;


[TestClass]
public class StateInstanceTests
{

    [TestMethod]
    public void StateInstance_RetrievesState_EachTimeInstanceInvoked()
    {
        var state = new TestState
        {
            StateValue = 1
        };

        var instance = new StateInstance<TestState>(() => state);

        Assert.AreEqual(1, instance.Instance.StateValue);

        state = new TestState
        {
            StateValue = 2
        };

        Assert.AreEqual(2, instance.Instance.StateValue);

        state = new TestState
        {
            StateValue = 3
        };

        Assert.AreEqual(3, instance.Instance.StateValue);

        state.Initialise();

        Assert.AreEqual(0, instance.Instance.StateValue);
    }

}
