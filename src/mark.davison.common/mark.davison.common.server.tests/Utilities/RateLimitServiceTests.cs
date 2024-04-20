namespace mark.davison.common.server.tests.Utilities;

[TestClass]
public class RateLimitServiceTests
{
    private readonly Mock<IDateService> _dateService;
    private readonly RateLimitService _service;
    private readonly DateTime _now;
    private readonly TimeSpan _delay;

    public RateLimitServiceTests()
    {
        _delay = TimeSpan.FromSeconds(10);
        _now = DateTime.UtcNow;
        _dateService = new(MockBehavior.Strict);
        _dateService.Setup(_ => _.Now).Returns(_now);
        _service = new(_delay, _dateService.Object);
        _dateService.Reset();
    }

    [TestMethod]
    public async Task Wait_WhereNoDelayRequired_DoesNotLimit()
    {
        _dateService
            .Setup(_ => _.Now)
            .Returns(_now.Add(_delay));

        var limited = await _service.Wait(CancellationToken.None);

        Assert.IsFalse(limited);
    }

    [TestMethod]
    public async Task Wait_WhereDelayRequired_DoesLimit()
    {
        _dateService
            .Setup(_ => _.Now)
            .Returns(_now);

        var limited = await _service.Wait(CancellationToken.None);

        Assert.IsTrue(limited);
    }
}
