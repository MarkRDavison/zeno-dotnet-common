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
}
