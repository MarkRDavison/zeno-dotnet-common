using mark.davison.common.Services;

namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class StandardZenoAuthenticationActionsTests
{
    private readonly Mock<IHttpRepository> _httpRepository;
    private readonly Mock<IDateService> _dateService;
    private readonly Mock<IZenoAuthenticationSession> _authSession;
    private readonly StandardZenoAuthenticationActions _actions;
    private User? _user = null;

    public StandardZenoAuthenticationActionsTests()
    {
        _httpRepository = new(MockBehavior.Strict);
        _dateService = new();
        _authSession = new(MockBehavior.Strict);
        _actions = new(_httpRepository.Object, _dateService.Object);

        _httpRepository
            .Setup(_ => _.GetEntityAsync<User>(
                It.IsAny<QueryParameters>(),
                It.IsAny<HeaderParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _user);

        _authSession
            .Setup(_ => _.SetString(
                ZenoAuthenticationConstants.SessionNames.User,
                It.IsAny<string>()));
    }

    [TestMethod]
    public async Task OnUserAuthenticated_WhereUserAndTokenDoNotExists_DoesNotSetUserInSession()
    {
        _authSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken))
            .Returns(string.Empty);

        _authSession
            .Setup(_ => _.SetString(
                ZenoAuthenticationConstants.SessionNames.User,
                It.IsAny<string>()))
            .Verifiable();

        await _actions.OnUserAuthenticated(new(), _authSession.Object, CancellationToken.None);

        _authSession
            .Verify(_ =>
                _.SetString(
                    ZenoAuthenticationConstants.SessionNames.User,
                    It.IsAny<string>()),
                Times.Never);
    }

    [TestMethod]
    public async Task OnUserAuthenticated_WhereUserDoesNotExist_AndTokenDoesExist_TriesToUpsertUser()
    {
        _authSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken))
            .Returns("TOKEN");

        _httpRepository
            .Setup(_ => _.UpsertEntityAsync(
                It.IsAny<User>(),
                It.IsAny<HeaderParameters>(),
                It.IsAny<CancellationToken>()))
            .Returns((User u, HeaderParameters h, CancellationToken c) => Task.FromResult<User?>(u))
            .Verifiable();

        await _actions.OnUserAuthenticated(new(), _authSession.Object, CancellationToken.None);

        _httpRepository
            .Verify(_ =>
                _.UpsertEntityAsync(
                    It.IsAny<User>(),
                    It.IsAny<HeaderParameters>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task OnUserAuthenticated_WhereUserUpsertFails_Throws()
    {
        _authSession
            .Setup(_ => _.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken))
            .Returns("TOKEN");

        _httpRepository
            .Setup(_ => _.UpsertEntityAsync(
                It.IsAny<User>(),
                It.IsAny<HeaderParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _actions.OnUserAuthenticated(new(), _authSession.Object, CancellationToken.None);

    }
}
