namespace mark.davison.common.server.tests.Health;

[TestClass]
public class GenericApplicationHealthStateHostedServiceTests
{
    private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetime;
    private readonly ApplicationHealthState _applicationHealthState;
    private readonly GenericApplicationHealthStateHostedService _service;

    public GenericApplicationHealthStateHostedServiceTests()
    {
        _hostApplicationLifetime = new(MockBehavior.Strict);
        _applicationHealthState = new();

        _service = new(_hostApplicationLifetime.Object, _applicationHealthState);
    }

    [TestMethod]
    public async Task StartAsync_RegistersAgainstApplicationLifeTimeEvents()
    {
        CancellationToken started = new();
        CancellationToken stopping = new();
        CancellationToken stopped = new();

        _hostApplicationLifetime.Setup(_ => _.ApplicationStarted).Returns(started);
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopping).Returns(stopping);
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopped).Returns(stopped);

        await _service.StartAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task Canceling_ApplicationStarted_SetsHealthStateToStarted()
    {
        CancellationTokenSource source = new CancellationTokenSource();

        _hostApplicationLifetime.Setup(_ => _.ApplicationStarted).Returns(source.Token);
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopping).Returns(new CancellationToken());
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopped).Returns(new CancellationToken());

        await _service.StartAsync(CancellationToken.None);

        source.Cancel();

        Assert.IsTrue(_applicationHealthState.Started);
    }

    [TestMethod]
    public async Task Canceling_ApplicationStopping_SetsHealthStateToNotReady()
    {
        CancellationTokenSource source = new CancellationTokenSource();

        _hostApplicationLifetime.Setup(_ => _.ApplicationStarted).Returns(new CancellationToken());
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopping).Returns(source.Token);
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopped).Returns(new CancellationToken());

        await _service.StartAsync(CancellationToken.None);

        source.Cancel();

        Assert.IsFalse(_applicationHealthState.Ready);
    }

    [TestMethod]
    public async Task Canceling_ApplicationStopped_SetsHealthStateToNotReady()
    {
        CancellationTokenSource source = new CancellationTokenSource();

        _hostApplicationLifetime.Setup(_ => _.ApplicationStarted).Returns(new CancellationToken());
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopping).Returns(new CancellationToken());
        _hostApplicationLifetime.Setup(_ => _.ApplicationStopped).Returns(source.Token);

        await _service.StartAsync(CancellationToken.None);

        source.Cancel();

        Assert.IsFalse(_applicationHealthState.Ready);
    }

    [TestMethod]
    public async Task StopAsync_SetsReadyStateToFalse()
    {
        await _service.StopAsync(CancellationToken.None);
        Assert.IsFalse(_applicationHealthState.Ready);
    }

}
