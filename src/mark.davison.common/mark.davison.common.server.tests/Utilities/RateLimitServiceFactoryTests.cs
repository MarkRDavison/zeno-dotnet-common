namespace mark.davison.common.server.tests.Utilities;

[TestClass]
public class RateLimitServiceFactoryTests
{
    private readonly Mock<IDateService> _dateService;
    private readonly RateLimitServiceFactory _factory;
    private readonly DateTime _now;

    public RateLimitServiceFactoryTests()
    {
        _now = DateTime.UtcNow;
        _dateService = new(MockBehavior.Strict);
        _dateService.Setup(_ => _.Now).Returns(_now);
        _factory = new(_dateService.Object);
    }

    [TestMethod]
    public void FactoryCanCreateRateLimiterGivenDelay()
    {
        var delay = TimeSpan.FromSeconds(2);
        var limiter = _factory.CreateRateLimiter(delay);

        Assert.AreEqual(delay, limiter.Delay);
    }
}
