namespace mark.davison.common.server.tests.Repository;

internal class TestHttpRepository : HttpRepository
{
    public TestHttpRepository(
        string remoteEndpoint,
        HttpClient httpClient
    ) : base(
        remoteEndpoint,
        httpClient)
    {
    }
}

[TestClass]
public class HttpRepositoryTests
{
    private HttpRepository _httpRepository = default!;
    private MockHttpMessageHandler _httpMessageHandler = default!;
    private string _remoteEndpoint = "https://localhost:8080/";

    [TestInitialize]
    public void TestInitialize()
    {
        _httpMessageHandler = new MockHttpMessageHandler();
        _httpRepository = new TestHttpRepository(_remoteEndpoint, new HttpClient(_httpMessageHandler));
    }

    [TestMethod]
    public async Task GetEntityAsync_ById_WhereUnsuccessfulResponse_ReturnsNull()
    {
        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        var entity = await _httpRepository.GetEntityAsync<TestEntity>(Guid.Empty, HeaderParameters.None, CancellationToken.None);

        Assert.IsNull(entity);
    }

    [TestMethod]
    public async Task GetEntityAsync_ById_WhereSuccessfulResponse_ReturnsEntity()
    {
        var repositoryEntity = new TestEntity();
        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(repositoryEntity))
        };

        var entity = await _httpRepository.GetEntityAsync<TestEntity>(Guid.Empty, HeaderParameters.None, CancellationToken.None);

        Assert.IsNotNull(entity);
        Assert.IsInstanceOfType(entity, typeof(TestEntity));
    }

    [TestMethod]
    public async Task GetEntityAsync_ById_WhereSuccessfulResponseWithNullContent_ReturnsNull()
    {
        var repositoryEntity = new TestEntity
        {
            Id = Guid.NewGuid()
        };
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api/{typeof(TestEntity).Name.ToLower()}/{repositoryEntity.Id}",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };
        };

        var entity = await _httpRepository.GetEntityAsync<TestEntity>(repositoryEntity.Id, HeaderParameters.None, CancellationToken.None);

        Assert.IsNull(entity);
    }

    [TestMethod]
    public async Task GetEntitiesAsync_ByPath_AppliesQueryStringFromQueryParameters()
    {
        var path = "/custom";
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api{path}?name=EntityName",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            };
        };

        var entities = await _httpRepository.GetEntitiesAsync<TestEntity>(path, query, HeaderParameters.None, CancellationToken.None);

        Assert.IsFalse(entities.Any());
    }

    [TestMethod]
    public async Task GetEntitiesAsync_AppliesQueryStringFromQueryParameters()
    {
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api/{nameof(TestEntity).ToLower()}?name=EntityName",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            };
        };

        var entities = await _httpRepository.GetEntitiesAsync<TestEntity>(query, HeaderParameters.None, CancellationToken.None);

        Assert.IsFalse(entities.Any());
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WhereInvalidResponse_ReturnsEmptyList()
    {
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        var entities = await _httpRepository.GetEntitiesAsync<TestEntity>(query, HeaderParameters.None, CancellationToken.None);

        Assert.IsFalse(entities.Any());
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WhereNullResponse_ReturnsEmptyList()
    {
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        var entities = await _httpRepository.GetEntitiesAsync<TestEntity>(query, HeaderParameters.None, CancellationToken.None);

        Assert.IsFalse(entities.Any());
    }

    [TestMethod]
    public async Task GetEntitiesAsync_ReturnsEntityList()
    {
        List<TestEntity> persistedEntities = new() {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api/{nameof(TestEntity).ToLower()}?name=EntityName",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(persistedEntities))
            };
        };

        var entities = await _httpRepository.GetEntitiesAsync<TestEntity>(query, HeaderParameters.None, CancellationToken.None);

        Assert.AreEqual(persistedEntities.Count, entities.Count);
    }

    [TestMethod]
    public async Task GetEntityAsync_ReturnsEntityList_AndSelectsFirst()
    {
        List<TestEntity> persistedEntities = new() {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };
        var query = new QueryParameters {
            { nameof(TestEntity.Name), "EntityName" }
        };
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api/{nameof(TestEntity).ToLower()}?name=EntityName",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(persistedEntities))
            };
        };

        var entity = await _httpRepository.GetEntityAsync<TestEntity>(query, HeaderParameters.None, CancellationToken.None);

        Assert.IsNotNull(entity);
        Assert.AreEqual(persistedEntities[0].Id, entity.Id);
    }

    [TestMethod]
    public async Task UpsertEntityAsync_WhereInvalidResponse_ReturnsNull()
    {
        var persistedEntity = new TestEntity();

        _httpMessageHandler.SendAsyncFunc = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        var entity = await _httpRepository.UpsertEntityAsync<TestEntity>(persistedEntity, HeaderParameters.None, CancellationToken.None);

        Assert.IsNull(entity);
    }

    [TestMethod]
    public async Task UpsertEntityAsync_WhereValidResponse_ReturnsEntity()
    {
        var persistedEntity = new TestEntity();

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual(
                $"{_remoteEndpoint.TrimEnd('/')}/api/{nameof(TestEntity).ToLower()}",
                _.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(persistedEntity))
            };
        };
        var entity = await _httpRepository.UpsertEntityAsync<TestEntity>(persistedEntity, HeaderParameters.None, CancellationToken.None);

        Assert.IsNotNull(entity);
    }
}
