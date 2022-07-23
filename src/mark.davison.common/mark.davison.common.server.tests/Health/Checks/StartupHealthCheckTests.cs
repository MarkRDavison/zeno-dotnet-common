namespace mark.davison.common.server.tests.Health.Checks;

[TestClass]
public class StartupHealthCheckTests
{

    private readonly Mock<IApplicationHealthState> _applicationHealthState;
    private readonly HealthCheckContext _healthCheckContext;
    private readonly StartupHealthCheck _healthCheck;

    public StartupHealthCheckTests()
    {
        _healthCheckContext = new();
        _applicationHealthState = new(MockBehavior.Strict);
        _healthCheck = new(_applicationHealthState.Object);
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenApplicationHealthStateStartedIsTrue()
    {
        _applicationHealthState.Setup(_ => _.Started).Returns(true);

        Assert.AreEqual(
            HealthCheckResult.Healthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenApplicationHealthStateStartedIsFalse()
    {
        _applicationHealthState.Setup(_ => _.Started).Returns(false);

        Assert.AreEqual(
            HealthCheckResult.Unhealthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenApplicationHealthStateStartedIsNull()
    {
        _applicationHealthState.Setup(_ => _.Started).Returns((bool?)null);

        Assert.AreEqual(
            HealthCheckResult.Unhealthy(),
            await _healthCheck.CheckHealthAsync(_healthCheckContext));
    }

    [TestMethod]
    public void Name_IsSameAsNameofClass()
    {
        Assert.AreEqual(nameof(StartupHealthCheck), StartupHealthCheck.Name);
    }
}
