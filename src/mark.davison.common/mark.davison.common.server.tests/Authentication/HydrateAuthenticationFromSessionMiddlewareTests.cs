namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class HydrateAuthenticationFromSessionMiddlewareTests
{
    private readonly Mock<ICurrentUserContext> _currentUserContext = new(MockBehavior.Strict);
    private readonly Mock<IZenoAuthenticationSession> _zenoAuthenticationSession = new(MockBehavior.Strict);
    private readonly Mock<IDateService> _dateService = new(MockBehavior.Strict);
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new(MockBehavior.Strict);
    private readonly ZenoAuthOptions _zenoAuthOptions = new();
    private readonly Mock<ILogger<HydrateAuthenticationFromSessionMiddleware>> _logger = new(MockBehavior.Loose);
    private readonly HydrateAuthenticationFromSessionMiddleware _middleware;
    private readonly HttpContext _context;
    private bool _nextInvoked;
    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;
    private string _user = string.Empty;
    private string _userProfile = string.Empty;

    public HydrateAuthenticationFromSessionMiddlewareTests()
    {
        _currentUserContext.SetupAllProperties();
        _middleware = new(_ =>
            {
                _nextInvoked = true;
                return Task.CompletedTask;
            },
            _zenoAuthenticationSession.Object,
            _dateService.Object,
            _httpClientFactory.Object,
            Options.Create(_zenoAuthOptions),
            _logger.Object);
        _context = new DefaultHttpContext();
        _dateService.Setup(_ => _.Now).Returns(DateTime.MinValue);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken))
            .Returns(() => _accessToken);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.RefreshToken))
            .Returns(() => _refreshToken);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.User))
            .Returns(() => _user);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.UserProfile))
            .Returns(() => _userProfile);
    }

    [DataTestMethod]
    [DataRow(null, "xy", "xy")]
    [DataRow("xy", null, "xy")]
    [DataRow("xy", "xy", null)]
    [DataRow("xy", null, null)]
    [DataRow(null, "xy", null)]
    [DataRow(null, null, "xy")]
    [DataRow(null, null, null)]
    public async Task Invoke_WhereNotAuthenticated_CallsNext(string? accessToken, string? user, string? userProfile)
    {
        _accessToken = accessToken ?? string.Empty;
        _user = user ?? string.Empty;
        _userProfile = userProfile ?? string.Empty;

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task Invoke_WhereSessionPopulatedWithInvalidSerialisedUser_DoesNotSetUser()
    {
        _accessToken = "TOKEN";
        _user = "INVALID_JSON";
        _userProfile = "INVALID_JSON";

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsFalse(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.sub)));

        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task Invoke_WhereSessionPopulatedWithInvalidSerialisedUserProfile_DoesNotSetUser()
    {
        var user = new User { };
        _accessToken = "TOKEN";
        _user = JsonSerializer.Serialize(user);
        _userProfile = "INVALID_JSON";

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsFalse(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.sub)));

        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task Invoke_WhereSessionPopulatedWithInvalidUserProfile_DoesNotSetUser()
    {
        var user = new User { };
        var userProfile = new UserProfile { };

        _accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _user = JsonSerializer.Serialize(user);
        _userProfile = JsonSerializer.Serialize(userProfile);

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsFalse(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.sub)));

        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task Invoke_WhereSessionPopulatedWithValidUserInfo_SetsUser()
    {
        var user = new User { Sub = Guid.NewGuid() };
        var userProfile = new UserProfile
        {
            sub = user.Sub,
            email_verified = true,
            email = "email@test.com",
            name = "test",
            preferred_username = "t.p",
            given_name = "test",
            family_name = "person"
        };

        _accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _refreshToken = "REFRESH";
        _user = JsonSerializer.Serialize(user);
        _userProfile = JsonSerializer.Serialize(userProfile);

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.sub)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.email_verified)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.name)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.preferred_username)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.given_name)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.family_name)));
        Assert.IsTrue(_context.User.HasClaim(_ => _.Type == nameof(UserProfile.email)));

        Assert.IsTrue(_nextInvoked);
    }
}
