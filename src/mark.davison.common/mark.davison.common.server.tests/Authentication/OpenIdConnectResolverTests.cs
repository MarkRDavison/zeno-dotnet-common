namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class OpenIdConnectResolverTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly ZenoAuthOptions _options;
    private readonly OpenIdConnectResolver _resolver;
    private readonly MockHttpMessageHandler _httpMessageHandler;

    public OpenIdConnectResolverTests()
    {
        _httpClientFactory = new(MockBehavior.Strict);
        _options = new ZenoAuthOptions
        {
            OpenIdConnectWellKnownUri = "https://wellknown/"
        };
        _resolver = new(_httpClientFactory.Object, _options);
        _httpMessageHandler = new();
    }

    [TestMethod]
    public async Task StartAsync_FetchesOpenIdConnectConfiguration()
    {
        var content = await File.ReadAllTextAsync("openid-configuration.json");

        _httpClientFactory
            .Setup(_ => _
                .CreateClient(ZenoAuthenticationConstants.AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler))
            .Verifiable();

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(_options.OpenIdConnectWellKnownUri, _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
        };

        await _resolver.StartAsync(CancellationToken.None);

        _httpClientFactory
            .Verify(_ => _
                .CreateClient(ZenoAuthenticationConstants.AuthClientName),
            Times.Once);

        Assert.AreEqual(
            "https://auth.markdavison.kiwi/auth/realms/markdavison.kiwi",
            _options.OpenIdConnectConfiguration.Issuer);
    }

    [TestMethod]
    public async Task StopAsync_DoesNothing()
    {
        await _resolver.StopAsync(CancellationToken.None);
    }
}
