namespace mark.davison.common.client.tests.CQRS;


[TestClass]
public class ActionDispatcherTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
    private readonly Mock<IActionHandler<ExampleAction>> _handler;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IServiceScope> _serviceScope;
    private readonly ActionDispatcher _actionDispatcher;

    public ActionDispatcherTests()
    {
        _serviceScopeFactory = new(MockBehavior.Strict);
        _handler = new(MockBehavior.Strict);
        _serviceProvider = new(MockBehavior.Strict);
        _serviceScope = new(MockBehavior.Strict);
        _actionDispatcher = new(_serviceScopeFactory.Object);
        _serviceScopeFactory.Setup(_ => _.CreateScope()).Returns(() => _serviceScope.Object);
        _serviceScope.Setup(_ => _.ServiceProvider).Returns(() => _serviceProvider.Object);
        _serviceScope.Setup(_ => _.Dispose());
    }

    [TestMethod]
    public async Task Dispatch_RetrievesRequiredServices()
    {
        _serviceProvider
            .Setup(_ => _
                .GetService(typeof(IActionHandler<ExampleAction>)))
            .Returns(_handler.Object)
            .Verifiable();

        _handler
            .Setup(_ => _
                .Handle(
                    It.IsAny<ExampleAction>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await _actionDispatcher.Dispatch<ExampleAction>(new ExampleAction(), CancellationToken.None);

        _handler
            .Verify(_ => _
                .Handle(
                    It.IsAny<ExampleAction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

        _serviceProvider.VerifyAll();
    }
}
