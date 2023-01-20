namespace mark.davison.common.server.integrationtests.Tests.Endpoints;

[TestClass]
public class GetEndpointsTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Comment> _existing = new();
    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IRepository>();
        var persisted = await repository.UpsertEntitiesAsync(new List<Comment> {
            new Comment { Id = Guid.NewGuid(), Content = "Comment #1" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #2" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #3" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #4", Date = DateOnly.FromDateTime(DateTime.Now) }
        });

        _existing.AddRange(persisted);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithoutFilter()
    {
        var comments = await GetMultipleAsync<Comment>("/api/comment");
        Assert.IsNotNull(comments);
        Assert.AreEqual(_existing.Count, comments.Count);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithRemoteLinqFilter()
    {
        var query = new QueryParameters();
        query.Where<Comment>(_ => _.Content == _existing.First().Content);
        var comments = await GetMultipleAsync<Comment>("/api/comment", query);
        Assert.IsNotNull(comments);
        Assert.AreEqual(1, comments.Count);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter()
    {
        var query = new QueryParameters
        {
            { nameof(Comment.Content), _existing.First().Content }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.AreEqual(1, comments.Count);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter_UsingDateOnly()
    {
        var dateString = DateOnly.FromDateTime(DateTime.Now).ToString();
        var query = new QueryParameters
        {
            { nameof(Comment.Date), dateString }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.AreEqual(1, comments.Count);
    }
}
