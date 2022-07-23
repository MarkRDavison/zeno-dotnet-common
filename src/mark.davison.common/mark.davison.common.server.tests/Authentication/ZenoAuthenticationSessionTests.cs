namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class ZenoAuthenticationSessionTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<ISession> _session;
    private readonly ZenoAuthenticationSession _zenoAuthenticationSession;
    private readonly HttpContext _httpContext;

    public ZenoAuthenticationSessionTests()
    {
        _httpContextAccessor = new(MockBehavior.Strict);
        _session = new(MockBehavior.Strict);
        _zenoAuthenticationSession = new(_httpContextAccessor.Object);
        _httpContext = new DefaultHttpContext
        {
            Session = _session.Object
        };
        _httpContextAccessor.Setup(_ => _.HttpContext).Returns(() => _httpContext);
    }

    [TestMethod]
    public async Task CommitSessionAsync_CallsUnderlyingSessionMethod()
    {
        _session
            .Setup(_ => _.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await _zenoAuthenticationSession.CommitSessionAsync(CancellationToken.None);

        _session
            .Verify(_ => _.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [TestMethod]
    public async Task LoadSessionAsync_CallsUnderlyingSessionMethod()
    {
        _session
            .Setup(_ => _.LoadAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await _zenoAuthenticationSession.LoadSessionAsync(CancellationToken.None);

        _session
            .Verify(_ => _.LoadAsync(It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [TestMethod]
    public void Clear_CallsUnderlyingSessionMethod()
    {
        _session
            .Setup(_ => _.Clear())
            .Verifiable();

        _zenoAuthenticationSession.Clear();

        _session
            .Verify(_ => _.Clear(),
                Times.Once);
    }

    [TestMethod]
    public void Remove_CallsUnderlyingSessionMethod()
    {
        string key = "key";
        _session
            .Setup(_ => _.Remove(key))
            .Verifiable();

        _zenoAuthenticationSession.Remove(key);

        _session
            .Verify(_ => _.Remove(key),
                Times.Once);
    }

    [TestMethod]
    public void GetString_CallsUnderlyingSessionMethod()
    {
        string key = "key";
        byte[]? data = null;
        _session
            .Setup(_ => _.TryGetValue(key, out data))
            .Returns(true)
            .Verifiable();

        _zenoAuthenticationSession.GetString(key);

        _session
            .Verify(_ => _.TryGetValue(key, out data),
                Times.Once);
    }

    [TestMethod]
    public void GetString_CallsUnderlyingSessionMethod_WhereReturnsFalse()
    {
        string key = "key";
        byte[]? data = null;
        _session
            .Setup(_ => _.TryGetValue(key, out data))
            .Returns(false)
            .Verifiable();

        var response = _zenoAuthenticationSession.GetString(key);
        Assert.AreEqual(string.Empty, response);

        _session
            .Verify(_ => _.TryGetValue(key, out data),
                Times.Once);
    }

    [TestMethod]
    public void SetString_CallsUnderlyingSessionMethod()
    {
        string key = "key";
        string valueStr = "value";

        _session
            .Setup(_ => _.Set(key, It.IsAny<byte[]>()))
            .Verifiable();

        _zenoAuthenticationSession.SetString(key, valueStr);

        _session
            .Verify(_ => _.Set(key, It.IsAny<byte[]>()),
                Times.Once);
    }
}
