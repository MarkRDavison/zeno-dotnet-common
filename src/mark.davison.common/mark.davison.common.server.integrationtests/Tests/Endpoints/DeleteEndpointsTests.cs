using mark.davison.common.server.abstractions.Authentication;
using mark.davison.common.server.Endpoints;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace mark.davison.common.server.integrationtests.Tests.Endpoints;

[TestClass]
public class DeleteEndpointsTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Comment> _existing = new();
    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IRepository>();
        var persisted = await repository.UpsertEntitiesAsync(new List<Comment> {
            new Comment { Id = Guid.NewGuid(), Content = "Comment #1" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #2" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #3" }
        });

        _existing.AddRange(persisted);
    }

    [TestMethod]
    public async Task Delete_Works()
    {
        await DeleteAsync($"/api/comment/{_existing.First().Id}", true);

        var deleted = await GetAsync<Comment>($"/api/comment/{_existing.First().Id}");
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task DeleteOnNonExistant_ReturnsNotFound()
    {
        var services = new ServiceCollection();

        Mock<IRepository> repository = new();
        Mock<ICurrentUserContext> currentUserContext = new();
        Mock<ILogger<Author>> logger = new();

        services.AddSingleton(repository.Object);
        services.AddSingleton(currentUserContext.Object);

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var response = await DeleteEndpoints.DeleteEntity(Guid.NewGuid(), context, logger.Object, CancellationToken.None) as IStatusCodeHttpResult;

        Assert.IsNotNull(response);
        Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteEntity_WhereDeleteFails_ReturnsUnprocessableEntity()
    {
        var services = new ServiceCollection();

        Mock<IRepository> repository = new();
        Mock<ICurrentUserContext> currentUserContext = new();
        Mock<ILogger<Comment>> logger = new();

        services.AddSingleton(repository.Object);
        services.AddSingleton(currentUserContext.Object);

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        repository
            .Setup(_ => _.GetEntityAsync<Comment>(
                _existing.First().Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existing.First());

        var response = await DeleteEndpoints.DeleteEntity(_existing.First().Id, context, logger.Object, CancellationToken.None) as IStatusCodeHttpResult;

        Assert.IsNotNull(response);
        Assert.AreEqual((int)HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
