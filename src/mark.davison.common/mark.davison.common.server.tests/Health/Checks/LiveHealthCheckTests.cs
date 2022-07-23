namespace mark.davison.common.server.tests.Health.Checks;

[TestClass]
public class LiveHealthCheckTests
{

    private readonly Mock<IApplicationHealthState> _applicationHealthState;
    private readonly HealthCheckContext _healthCheckContext;
    private readonly LiveHealthCheck _healthCheck;

    public LiveHealthCheckTests()
    {
        _healthCheckContext = new();
        _applicationHealthState = new(MockBehavior.Strict);
        _healthCheck = new(_applicationHealthState.Object);
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenApplicationHealthStateReadyIsTrue()
    {
        _applicationHealthState.Setup(_ => _.Ready).Returns(true);

        Assert.AreEqual(
            HealthCheckResult.Healthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenApplicationHealthStateReadyIsFalse()
    {
        _applicationHealthState.Setup(_ => _.Ready).Returns(false);

        Assert.AreEqual(
            HealthCheckResult.Unhealthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenApplicationHealthStateReadyIsNull()
    {
        _applicationHealthState.Setup(_ => _.Ready).Returns((bool?)null);

        Assert.AreEqual(
            HealthCheckResult.Unhealthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public void Name_IsSameAsNameofClass()
    {
        Assert.AreEqual(nameof(LiveHealthCheck), LiveHealthCheck.Name);
    }
}
