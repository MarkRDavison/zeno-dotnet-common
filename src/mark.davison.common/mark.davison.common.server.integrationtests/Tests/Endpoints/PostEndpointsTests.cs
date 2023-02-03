using mark.davison.common.persistence.EntityDefaulter;
using mark.davison.common.server.abstractions.Authentication;
using mark.davison.common.server.abstractions.Identification;
using mark.davison.common.server.Endpoints;
using mark.davison.common.server.integrationtests.Tests.Defaulters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using Moq;
using System.Net;

namespace mark.davison.common.server.integrationtests.Tests.Endpoints;

[TestClass]
public class PostEndpointsTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    [TestMethod]
    public async Task Post_CreatesEntity()
    {
        var newComment = new Comment
        {
            Id = Guid.NewGuid()
        };

        var comment = await UpsertAsync("/api/comment", newComment);

        Assert.IsNotNull(comment);
        Assert.AreEqual(newComment.Id, comment.Id);
        Assert.AreEqual(newComment.Content, comment.Content);

        comment = await GetAsync<Comment>("/api/comment/" + newComment.Id.ToString());

        Assert.IsNotNull(comment);
        Assert.AreEqual(newComment.Id, comment.Id);
        Assert.AreEqual(newComment.Content, comment.Content);
    }

    [TestMethod]
    public async Task Post_WhereEntityDefaulterExists_InvokesDefaultAsync()
    {
        var author = new Author
        {
            Id = Guid.NewGuid()
        };

        var upserted = await UpsertAsync("/api/author", author);

        Assert.AreEqual(AuthorDefaulter.LAST_NAME, upserted?.LastName);
    }

    [TestMethod]
    public async Task PostEntity_WhereUpsertFails_ReturnsUnprocessableEntity()
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

        var response = await PostEndpoints.PostEntity(new Author(), context, logger.Object, CancellationToken.None) as IStatusCodeHttpResult;

        Assert.IsNotNull(response);
        Assert.AreEqual((int)HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
