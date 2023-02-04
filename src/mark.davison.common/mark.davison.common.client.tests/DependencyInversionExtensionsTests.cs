namespace mark.davison.common.client.tests;

[TestClass]
public class DependencyInversionExtensionsTests
{
    private readonly IServiceCollection _serviceCollection;

    public DependencyInversionExtensionsTests()
    {
        _serviceCollection = new ServiceCollection();
    }

    [TestMethod]
    public void UseCQRS_RegistersExpectedHandlers()
    {
        _serviceCollection.UseCQRSClient();

        var provider = _serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsNotNull(scope.ServiceProvider.GetService<ICommandHandler<ExampleCommandRequest, ExampleCommandResponse>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IQueryHandler<ExampleQueryRequest, ExampleQueryResponse>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IActionHandler<ExampleAction>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IResponseActionHandler<ExampleResponseActionRequest, ExampleResponseActionResponse>>());
    }
}
