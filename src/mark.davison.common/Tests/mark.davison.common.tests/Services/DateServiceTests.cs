namespace mark.davison.common.tests.Services;

[TestClass]
public class DateServiceTests
{
    [TestMethod]
    public void Today_InLocalMode_ReturnsCorrectly()
    {
        DateService _dateService = new(DateService.DateMode.Local);

        var now = _dateService.Today;

        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Today), now);
    }

    [TestMethod]
    public void Today_InUtcMode_ReturnsCorrectly()
    {
        DateService _dateService = new(DateService.DateMode.Utc);

        var now = _dateService.Today;

        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), now);
    }

    [TestMethod]
    public void Now_InLocalMode_ReturnsCorrectly()
    {
        var start = DateTime.Now;
        DateService _dateService = new(DateService.DateMode.Local);

        var now = _dateService.Now;

        Assert.IsTrue(start <= now);
        Assert.IsTrue(now <= DateTime.Now);
    }

    [TestMethod]
    public void Now_InUtcMode_ReturnsCorrectly()
    {
        var start = DateTime.UtcNow;
        DateService _dateService = new(DateService.DateMode.Utc);

        var now = _dateService.Now;

        Assert.IsTrue(start <= now);
        Assert.IsTrue(now <= DateTime.UtcNow);
    }
}
