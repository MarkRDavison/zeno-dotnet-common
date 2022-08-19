namespace mark.davison.common.client.tests.Repository;

internal class TestClientHttpRepository : ClientHttpRepository
{
    public TestClientHttpRepository(
        string remoteEndpoint,
        HttpClient httpClient
    ) : base(
        remoteEndpoint,
        httpClient)
    {
    }
}

internal class TestGetResponse
{
    public string TestValue { get; set; } = string.Empty;
}

internal class TestPostResponse
{
    public string TestValue { get; set; } = string.Empty;
}

[GetRequest(Path = "/get")]
internal class TestGetRequest : IQuery<TestGetRequest, TestGetResponse>, ICommand<TestGetRequest, TestGetResponse>
{
    public string Value { get; set; } = string.Empty;
    public string? NullableValue { get; set; }
}

[PostRequest(Path = "/post")]
internal class TestPostRequest : ICommand<TestPostRequest, TestPostResponse>, IQuery<TestPostRequest, TestPostResponse>
{

}

[TestClass]
public class ClientHttpRepositoryTests
{
    private ClientHttpRepository _clientHttpRepository = default!;
    private MockHttpMessageHandler _httpMessageHandler = default!;
    private string _remoteEndpoint = "https://localhost:8080/";

    [TestInitialize]
    public void TestInitialize()
    {
        _httpMessageHandler = new MockHttpMessageHandler();
        _clientHttpRepository = new TestClientHttpRepository(_remoteEndpoint, new HttpClient(_httpMessageHandler));
    }

    [TestMethod]
    public async Task Get_ByRequest_RetrievesResponseViaAttributePath()
    {
        var expectedResponse = new TestGetResponse
        {
            TestValue = "abcdefghijklmnopqrstuvwxy"
        };

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/get", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
        };

        var response = await _clientHttpRepository
            .Get<TestGetResponse, TestGetRequest>(
                new TestGetRequest(),
                CancellationToken.None);

        Assert.AreEqual(expectedResponse.TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Get_PopulatesQueryParameter()
    {
        var expectedResponse = new TestGetResponse
        {
            TestValue = "abcdefghijklmnopqrstuvwxy"
        };
        var request = new TestGetRequest
        {
            Value = "abc"
        };

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/get?value=abc", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
        };

        var response = await _clientHttpRepository
            .Get<TestGetResponse, TestGetRequest>(
                request,
                CancellationToken.None);

        Assert.AreEqual(expectedResponse.TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Get_ByType_RetrievesResponseViaAttributePath()
    {
        var expectedResponse = new TestGetResponse
        {
            TestValue = "abcdefghijklmnopqrstuvwxy"
        };

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/get", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
        };

        var response = await _clientHttpRepository
            .Get<TestGetResponse, TestGetRequest>(CancellationToken.None);

        Assert.AreEqual(expectedResponse.TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Get_WhereNullValueReturned_CreatesNewResponse()
    {
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/get", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };
        };

        var response = await _clientHttpRepository
            .Get<TestGetResponse, TestGetRequest>(CancellationToken.None);

        Assert.AreEqual(new TestGetResponse().TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Post_ByRequest_RetrievesResponseViaAttributePath()
    {
        var expectedResponse = new TestPostResponse
        {
            TestValue = "abcdefghijklmnopqrstuvwxy"
        };

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/post", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
        };

        var response = await _clientHttpRepository
            .Post<TestPostResponse, TestPostRequest>(
                new TestPostRequest(),
                CancellationToken.None);

        Assert.AreEqual(expectedResponse.TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Post_ByType_RetrievesResponseViaAttributePath()
    {
        var expectedResponse = new TestPostResponse
        {
            TestValue = "abcdefghijklmnopqrstuvwxy"
        };

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/post", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
        };

        var response = await _clientHttpRepository
            .Post<TestPostResponse, TestPostRequest>(CancellationToken.None);

        Assert.AreEqual(expectedResponse.TestValue, response.TestValue);
    }

    [TestMethod]
    public async Task Post_WhereNullValueReturned_CreatesNewResponse()
    {
        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            Assert.AreEqual($"{_remoteEndpoint.Trim('/')}/api/post", _.RequestUri?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };
        };

        var response = await _clientHttpRepository
            .Post<TestPostResponse, TestPostRequest>(CancellationToken.None);

        Assert.AreEqual(new TestPostResponse().TestValue, response.TestValue);
    }

    [TestMethod, ExpectedException(typeof(InvalidOperationException))]
    public async Task Get_WhereAttributeNotPresent_Throws()
    {
        await _clientHttpRepository
            .Get<TestPostResponse, TestPostRequest>(CancellationToken.None);
    }

    [TestMethod, ExpectedException(typeof(InvalidOperationException))]
    public async Task Post_WhereAttributeNotPresent_Throws()
    {
        await _clientHttpRepository
            .Post<TestGetResponse, TestGetRequest>(CancellationToken.None);
    }
}
