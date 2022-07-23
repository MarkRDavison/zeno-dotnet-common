namespace mark.davison.common.server.tests.Health;

[TestClass]
public class ApplicationHealthStateTests
{
    private readonly ApplicationHealthState _applicationHealthState = new();

    [TestMethod]
    public void Started_IsNullInitially()
    {
        Assert.IsNull(_applicationHealthState.Started);
    }

    [TestMethod]
    public void Ready_IsNullInitially()
    {
        Assert.IsNull(_applicationHealthState.Ready);
    }

    [TestMethod]
    public void Healthy_IsTrueInitially()
    {
        Assert.IsTrue(_applicationHealthState.Healthy);
    }

}
