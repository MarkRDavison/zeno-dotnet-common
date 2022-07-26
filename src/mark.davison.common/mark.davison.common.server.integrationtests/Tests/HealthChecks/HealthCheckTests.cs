namespace mark.davison.common.server.integrationtests.Tests.HealthChecks;

[TestClass]
public class HealthCheckTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    [TestMethod]
    public async Task Ready_ReturnsHealthy()
    {
        var response = await GetRawAsync("/health/readiness");
        Assert.AreEqual("Healthy", response);
        response = await GetRawAsync("/health/liveness");
        Assert.AreEqual("Healthy", response);
        response = await GetRawAsync("/health/startup");
        Assert.AreEqual("Healthy", response);
    }
}
