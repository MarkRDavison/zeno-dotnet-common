﻿namespace mark.davison.common.server.integrationtests.Tests.Repository;

[TestClass]
public class HttpRepositoryTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Author> _authors = new()
    {
        new Author { Id = Guid.NewGuid(), FirstName = "a" },
        new Author { Id = Guid.NewGuid(), FirstName = "ab" },
        new Author { Id = Guid.NewGuid(), FirstName = "b" },
        new Author { Id = Guid.NewGuid(), FirstName = "ba" },
    };

    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            await repository.UpsertEntitiesAsync(_authors);
        }
    }

    [TestMethod]
    public async Task HttpRepository_GetEntities_WithRemoteLinqWhereClause_Works()
    {
        Expression<Func<Author, bool>> expression = _ => _.FirstName.StartsWith("a");
        var httpRepository = Services.GetRequiredService<IHttpRepository>();

        var query = new QueryParameters();
        query.Where(expression);

        var authors = await httpRepository.GetEntitiesAsync<Author>(query, HeaderParameters.None, CancellationToken.None);

        Assert.AreEqual(_authors.Where(expression.Compile()).Count(), authors.Count());
    }

    [TestMethod]
    public void CreateUriFromRelative_ForRelativeEndpoint_CreatesUriCorrectly()
    {
        string remoteEndpoint = "";
        string relativeUri = "/api/comment";

        var repo = new SampleHttpRepository(remoteEndpoint, new JsonSerializerOptions());

        var uri = repo.CreateUriFromRelative(relativeUri);

        Assert.AreEqual(relativeUri, uri.ToString());
    }

    [TestMethod]
    public void CreateUriFromRelative_ForAbsoluteEndpoint_CreatesUriCorrectly()
    {
        string remoteEndpoint = "https://localhost";
        string relativeUri = "/api/comment";

        var repo = new SampleHttpRepository(remoteEndpoint, new JsonSerializerOptions());

        var uri = repo.CreateUriFromRelative(relativeUri);

        Assert.AreEqual($"{remoteEndpoint}{relativeUri}", uri.ToString());
    }
}

