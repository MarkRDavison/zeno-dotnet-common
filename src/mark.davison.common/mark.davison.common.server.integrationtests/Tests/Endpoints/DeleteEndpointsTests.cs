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
}
