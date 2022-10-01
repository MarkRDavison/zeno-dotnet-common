namespace mark.davison.common.persistence.tests.Controllers;


[TestClass]
public class BaseControllerTests
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureRemoteLinq();

    private class AuthorController : BaseController<Author>
    {
        public AuthorController(
            ILogger logger,
            IRepository repository,
            IServiceScopeFactory serviceScopeFactory,
            ICurrentUserContext currentUserContext
        ) : base(
            logger,
            repository,
            serviceScopeFactory,
            currentUserContext)
        {
        }

        protected override void PatchUpdate(Author persisted, Author patched)
        {
            throw new NotImplementedException();
        }
    }

    private readonly AuthorController _controller;
    private readonly Mock<ILogger> _logger;
    private readonly Mock<IRepository> _repository;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
    private readonly Mock<ICurrentUserContext> _currentUserContext;

    public BaseControllerTests()
    {
        _logger = new(MockBehavior.Loose);
        _repository = new(MockBehavior.Strict);
        _serviceScopeFactory = new(MockBehavior.Strict);
        _currentUserContext = new(MockBehavior.Strict);

        _controller = new(
            _logger.Object,
            _repository.Object,
            _serviceScopeFactory.Object,
            _currentUserContext.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [TestMethod]
    public async Task Get_WithSerializedWhereClause_InvokesRepositoryCorrectly()
    {
        Expression<Func<Author, bool>> where =
            _ =>
                _.FirstName.StartsWith("a") ||
                _.FirstName.StartsWith("b") ||
                _.FirstName.Contains("e");

        var authors = new List<Author>
        {
            new() { FirstName = "a" },
            new() { FirstName = "b" },
            new() { FirstName = "c" },
            new() { FirstName = "d" },
            new() { FirstName = "e" },
            new() { FirstName = "f" },
            new() { FirstName = "g" },
        };

        var queryParams = new QueryParameters();
        queryParams.Where(where);

        using var memoryStream = new MemoryStream(
            Encoding.UTF8.GetBytes(queryParams.CreateBody()));

        _controller.ControllerContext.HttpContext.Request.Body = memoryStream;

        _repository
            .Setup(_ => _.GetEntitiesAsync<Author>(
                It.IsAny<Expression<Func<Author, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Author, bool>> w, string i, CancellationToken c) =>
            {
                var expr1 = where.Compile();
                var expr2 = w.Compile();

                foreach (var a in authors)
                {
                    Assert.AreEqual(expr1(a), expr2(a));
                }

                return new();
            })
            .Verifiable();

        await _controller.Get(CancellationToken.None);

        _repository
            .Verify(
                _ => _.GetEntitiesAsync<Author>(
                    It.IsAny<Expression<Func<Author, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }
}
