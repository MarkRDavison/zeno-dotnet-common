namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class HydrateAuthenticationFromClaimsPrincipalMiddlewareTests
{
    private readonly Mock<IRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<ICurrentUserContext> _currentUserContext = new(MockBehavior.Strict);
    private readonly HydrateAuthenticationFromClaimsPrincipalMiddleware _middleware;
    private readonly HttpContext _context;
    private bool _nextInvoked;

    public class AsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    public HydrateAuthenticationFromClaimsPrincipalMiddlewareTests()
    {
        _currentUserContext.SetupAllProperties();
        _middleware = new(_ =>
        {
            _nextInvoked = true;
            return Task.CompletedTask;
        });
        _context = new DefaultHttpContext();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IRepository>(_ => _repository.Object);

        _context.RequestServices = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public async Task Invoke_WhereIdentityNotAuthenticated_CallsNext()
    {
        await _middleware.Invoke(_context, _currentUserContext.Object);
        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task Invoke_WhereIdentityIsNull_CallsNext()
    {
        _context.User = new ClaimsPrincipal();
        await _middleware.Invoke(_context, _currentUserContext.Object);
        Assert.IsTrue(_nextInvoked);
    }

    [TestMethod]
    public async Task AuthenticatedInvoke_WhereHeaderNotPresent_RetrievesFromClaimsAndRepository()
    {
        var persistedUser = new User
        {
            Id = Guid.NewGuid(),
            Sub = Guid.NewGuid()
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "example name"),
            new Claim(ClaimTypes.NameIdentifier, persistedUser.Sub.ToString()),
        }, "mock"));
        _context.User = user;

        _repository.Setup(_ => _.BeginTransaction()).Returns(new AsyncDisposable());

        _repository
            .Setup(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(() => persistedUser)
            .Verifiable();

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_nextInvoked);

        _repository
            .Verify(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticatedInvoke_WhereHeaderPresent_DoesNotRetrieveFromRepository()
    {
        var persistedUser = new User
        {
            Id = Guid.NewGuid(),
            Sub = Guid.NewGuid()
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "example name"),
            new Claim(ClaimTypes.NameIdentifier, persistedUser.Sub.ToString()),
        }, "mock"));
        _context.User = user;
        _context.Request.Headers.Add(ZenoAuthenticationConstants.HeaderNames.User, JsonSerializer.Serialize(persistedUser));

        _repository
            .Setup(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Verifiable();

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_nextInvoked);

        _repository
            .Verify(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [TestMethod]
    public async Task AuthenticatedInvoke_WhereHeaderNotPresent_RetrievesFromClaims_ButNotFromRepositoryIfGuidInvalid()
    {
        var persistedUser = new User
        {
            Id = Guid.NewGuid(),
            Sub = Guid.NewGuid()
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "example name"),
            new Claim(ClaimTypes.NameIdentifier, "An Invalid Guid Value"),
        }, "mock"));
        _context.User = user;

        _repository
            .Setup(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Verifiable();

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_nextInvoked);

        _repository
            .Verify(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [TestMethod]
    public async Task AuthenticatedInvoke_WhereHeaderNotPresent_RetrievesFromClaims_ButNotFromRepositoryIfClaimNotPresent()
    {
        var persistedUser = new User
        {
            Id = Guid.NewGuid(),
            Sub = Guid.NewGuid()
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "example name"),
        }, "mock"));
        _context.User = user;

        _repository
            .Setup(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Verifiable();

        await _middleware.Invoke(_context, _currentUserContext.Object);

        Assert.IsTrue(_nextInvoked);

        _repository
            .Verify(_ => _
            .GetEntityAsync<User>(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }
}
