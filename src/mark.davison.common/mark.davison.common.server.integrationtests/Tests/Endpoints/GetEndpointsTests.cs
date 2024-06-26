﻿namespace mark.davison.common.server.integrationtests.Tests.Endpoints;

[TestClass]
public class GetEndpointsTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Comment> _existing = new();
    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            var persisted = await repository.UpsertEntitiesAsync(new List<Comment> {
            new Comment { Id = Guid.NewGuid(), Content = "Comment #1" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #2" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #3" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #4", Date = DateOnly.FromDateTime(DateTime.Now) },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #5", Integer = 5 },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #6", Long = 5 },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #7", Guid = Guid.NewGuid()},
            new Comment { Id = Guid.NewGuid(), Content = "Comment #8", Long = 5, Integer = 6 },
        });

            _existing.AddRange(persisted);
        }
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithoutFilter()
    {
        var comments = await GetMultipleAsync<Comment>("/api/comment");
        Assert.IsNotNull(comments);
        Assert.AreEqual(_existing.Count, comments.Count);
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
        Assert.IsTrue(comments.Count >= 1);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter_UsingGuid()
    {
        var guidParameter = _existing.First(_ => _.Guid != Guid.Empty).Guid;
        var query = new QueryParameters
        {
            { nameof(Comment.Guid), guidParameter.ToString() }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.IsTrue(comments.Count >= 1);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter_UsingLong()
    {
        var longParameter = _existing.First(_ => _.Long != 0).Long;
        var query = new QueryParameters
        {
            { nameof(Comment.Long), longParameter.ToString() }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.IsTrue(comments.Count >= 1);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter_UsingInt()
    {
        var intParameter = _existing.First(_ => _.Integer != 0).Integer;
        var query = new QueryParameters
        {
            { nameof(Comment.Integer), intParameter.ToString() }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.IsTrue(comments.Count >= 1);
    }

    [TestMethod]
    public async Task GetAll_ReturnsCorrectly_WithQueryParamFilter_UsingIntAndLong()
    {
        var comment = _existing.First(_ => _.Integer != 0 && _.Long != 0);

        var intParameter = comment.Integer;
        var longParameter = comment.Long;

        var query = new QueryParameters
        {
            { nameof(Comment.Integer), intParameter.ToString() },
            { nameof(Comment.Long), longParameter.ToString() }
        };
        var comments = await GetMultipleAsync<Comment>($"/api/comment{query.CreateQueryString()}");
        Assert.IsNotNull(comments);
        Assert.IsTrue(comments.Count >= 1);
    }
}
