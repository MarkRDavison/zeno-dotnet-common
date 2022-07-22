namespace mark.davison.common.client.tests.Authentication;

[TestClass]
public class AuthenticationContextTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<IAuthenticationConfig> _authenticationConfig;
    private readonly MockNavigationManager _navigationManager;
    private readonly MockHttpMessageHandler _httpMessageHandler;
    private readonly AuthenticationContext _authenticationContext;

    private string uri = "/current-page";
    private string baseUri = "http://localhost:8080/";

    public AuthenticationContextTests()
    {
        _httpClientFactory = new(MockBehavior.Strict); ;
        _authenticationConfig = new(MockBehavior.Strict);
        _navigationManager = new MockNavigationManager(baseUri, baseUri.Trim('/') + uri);
        _authenticationContext = new(_httpClientFactory.Object, _authenticationConfig.Object, _navigationManager);
        _httpMessageHandler = new();
    }

    [TestMethod]
    public async Task Login_NavigatesNavigationManager_ToLoginEndpoint()
    {
        var loginEndpoint = baseUri + "login";

        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(loginEndpoint);

        await _authenticationContext.Login();

        Assert.IsTrue(_navigationManager.NavigateToLocation!.Contains(loginEndpoint));
    }

    [TestMethod]
    public async Task Login_RedirectUriParameter_IncludesCurrentUri()
    {
        var loginEndpoint = baseUri + "login";

        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(loginEndpoint);

        await _authenticationContext.Login();

        Assert.IsTrue(_navigationManager.NavigateToLocation!.Contains(loginEndpoint));
        Assert.IsTrue(_navigationManager.NavigateToLocation!.Contains($"?redirect_uri={uri}"));
    }

    [TestMethod]
    public async Task Logout_NavigatesNavigationManager_ToLogoutEndpoint()
    {
        var logoutEndpoint = baseUri + "logout";

        _authenticationConfig.Setup(_ => _.LogoutEndpoint).Returns(logoutEndpoint);

        await _authenticationContext.Logout();

        Assert.AreEqual(logoutEndpoint, _navigationManager.NavigateToLocation);
    }

    [TestMethod]
    public async Task ValidateAuthState_WhereCreateClientThrows_SetsAuthenticatingToFalse()
    {
        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(baseUri + "login");

        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Throws(new Exception());

        await _authenticationContext.ValidateAuthState();

        Assert.IsFalse(_authenticationContext.IsAuthenticating);
    }

    [TestMethod]
    public async Task ValidateAuthState_WhereCreateClientThrows_RedirectsToLogin()
    {
        var loginEndpoint = baseUri + "login";

        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(loginEndpoint);
        _authenticationConfig.Setup(_ => _.UserEndpoint).Returns(baseUri + "user");

        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Throws(new Exception());

        await _authenticationContext.ValidateAuthState();

        Assert.IsTrue(_navigationManager.NavigateToLocation!.Contains(loginEndpoint));
    }

    [TestMethod]
    public async Task ValidateAuthState_WhereClientRequestUnsuccessful_SetsAuthenticatingToFalse()
    {
        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(baseUri + "login");
        _authenticationConfig.Setup(_ => _.UserEndpoint).Returns(baseUri + "user");

        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized);

        await _authenticationContext.ValidateAuthState();

        Assert.IsFalse(_authenticationContext.IsAuthenticating);
    }

    [TestMethod]
    public async Task ValidateAuthState_WhereClientRequestReturnsNullUserProfile_SetsAuthenticatingToFalse()
    {
        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(baseUri + "login");
        _authenticationConfig.Setup(_ => _.UserEndpoint).Returns(baseUri + "user");

        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        await _authenticationContext.ValidateAuthState();

        Assert.IsFalse(_authenticationContext.IsAuthenticating);
    }

    [TestMethod]
    public async Task ValidateAuthState_WhereClientRequestReturnsUserProfile_SetsAuthenticatedToTrue()
    {
        _authenticationConfig.Setup(_ => _.LoginEndpoint).Returns(baseUri + "login");
        _authenticationConfig.Setup(_ => _.UserEndpoint).Returns(baseUri + "user");

        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new UserProfile()))
        };

        await _authenticationContext.ValidateAuthState();

        Assert.IsFalse(_authenticationContext.IsAuthenticating);
        Assert.IsTrue(_authenticationContext.IsAuthenticated);
    }
}
