using mark.davison.common.server.tests.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace mark.davison.common.server.tests;

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
        _serviceCollection.UseCQRS(typeof(DependencyInversionExtensionsTests));

        var provider = _serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsNotNull(scope.ServiceProvider.GetService<ICommandHandler<ExampleCommandRequest, ExampleCommandResponse>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IQueryHandler<ExampleQueryRequest, ExampleQueryResponse>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<ICommandHandler<ListStateCommandRequest<ListStateItem>, ListStateCommandResponse<ListStateItem>>>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IQueryHandler<ListStateQueryRequest<ListStateItem>, ListStateQueryResponse<ListStateItem>>>());
    }
}
