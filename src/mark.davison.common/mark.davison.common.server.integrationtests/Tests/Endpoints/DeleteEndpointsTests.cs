namespace mark.davison.common.server.integrationtests.Tests.Endpoints;

[TestClass]
public class DeleteEndpointsTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Comment> _existing = new();
    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<IDbContext>();

        _existing.Clear();
        _existing.AddRange(new List<Comment> {
            new Comment { Id = Guid.NewGuid(), Content = "Comment #1" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #2" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment #3" }
            });

        foreach (var c in _existing)
        {
            await dbContext.Set<Comment>().AddAsync(c);
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
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
        var id = Guid.NewGuid();

        var status = await DeleteAsync($"/api/comment/{id}");

        Assert.AreEqual(HttpStatusCode.NotFound, status);
    }

}
