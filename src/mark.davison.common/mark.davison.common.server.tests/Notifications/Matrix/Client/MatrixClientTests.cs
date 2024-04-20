namespace mark.davison.common.server.tests.Notifications.Matrix.Client;

[TestClass]
public class MatrixClientTests
{
    private readonly MatrixClient _matrixClient;
    private readonly Mock<IHttpClientFactory> _factory;
    private readonly HttpClient _httpClient;
    private readonly MatrixNotificationSettings _settings;
    private readonly TestHttpMessageHandler _httpMessageHandler;
    private readonly JsonSerializerOptions _serializerOptions;

    public MatrixClientTests()
    {
        _httpMessageHandler = new();
        _httpClient = new(_httpMessageHandler);
        _factory = new(MockBehavior.Strict);

        _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _factory
            .Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _settings = new()
        {
            URL = "https://matrix-client.matrix.org",
            ROOMID = "!theroomid:matrix.org",
            USERNAME = "@theusername:matrix.org",
            PASSWORD = "thepassword"
        };

        _matrixClient = new(Options.Create(_settings), _factory.Object);
    }

    [TestMethod]
    public async Task SendMessage_WhenAuthNotPresent_LogsIn()
    {
        bool loginHit = false;
        bool sendMessageHit = false;
        var accessToken = "validtoken";
        _httpMessageHandler.Callback = (HttpRequestMessage requestMessage) =>
        {
            var responseMessage = new HttpResponseMessage();

            if (requestMessage.RequestUri!.ToString().Contains(MatrixConstants.LoginPath))
            {
                loginHit = true;

                responseMessage.Content = JsonContent.Create(new LoginResponse
                {
                    AccessToken = accessToken
                }, options: _serializerOptions);
                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                sendMessageHit = true;
                Assert.IsNotNull(requestMessage.Headers.Authorization?.Parameter);
                Assert.AreEqual(accessToken, requestMessage.Headers.Authorization.Parameter);
            }

            return Task.FromResult(responseMessage);
        };

        var message = "Hello world from C#";
        var response = await _matrixClient.SendMessage(_settings.ROOMID, message);

        Assert.IsTrue(response.Success);
        Assert.IsTrue(loginHit);
        Assert.IsTrue(sendMessageHit);
    }

    [TestMethod]
    public async Task SendMessage_WhenAuthPresent_DoesNotLogIn()
    {
        var loginHitCount = 0;
        var sendMessageHitCount = 0;
        var accessToken = "validtoken";
        _httpMessageHandler.Callback = (HttpRequestMessage requestMessage) =>
        {
            var responseMessage = new HttpResponseMessage();

            if (requestMessage.RequestUri!.ToString().Contains(MatrixConstants.LoginPath))
            {
                loginHitCount++;

                responseMessage.Content = JsonContent.Create(new LoginResponse
                {
                    AccessToken = accessToken
                }, options: _serializerOptions);
                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                sendMessageHitCount++;
                Assert.IsNotNull(requestMessage.Headers.Authorization?.Parameter);
                Assert.AreEqual(accessToken, requestMessage.Headers.Authorization.Parameter);
            }

            return Task.FromResult(responseMessage);
        };

        var message = "Hello world from C#";

        var response = await _matrixClient.SendMessage(_settings.ROOMID, message);
        Assert.IsTrue(response.Success);

        response = await _matrixClient.SendMessage(_settings.ROOMID, message);
        Assert.IsTrue(response.Success);

        Assert.AreEqual(1, loginHitCount);
        Assert.AreEqual(2, sendMessageHitCount);
    }

    [TestMethod]
    public async Task SendMessage_WhenAuthPresentButInvalid_AttemptsToLogIn()
    {
        var validSendMessageHit = false;
        var loginHitCount = 0;
        var sendMessageHitCount = 0;
        var accessToken = "validtoken";
        var invalidAccessToken = "invalidtoken";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalidAccessToken);

        _httpMessageHandler.Callback = (HttpRequestMessage requestMessage) =>
        {
            var responseMessage = new HttpResponseMessage();

            if (requestMessage.RequestUri!.ToString().Contains(MatrixConstants.LoginPath))
            {
                loginHitCount++;

                responseMessage.Content = JsonContent.Create(new LoginResponse
                {
                    AccessToken = accessToken
                }, options: _serializerOptions);
                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                sendMessageHitCount++;

                if (requestMessage.Headers.Authorization?.Parameter == invalidAccessToken)
                {
                    responseMessage.StatusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    Assert.IsNotNull(requestMessage.Headers.Authorization?.Parameter);
                    Assert.AreEqual(accessToken, requestMessage.Headers.Authorization.Parameter);
                    validSendMessageHit = true;
                }
            }

            return Task.FromResult(responseMessage);
        };

        var message = "Hello world from C#";

        var response = await _matrixClient.SendMessage(_settings.ROOMID, message);
        Assert.IsTrue(response.Success);

        Assert.AreEqual(1, loginHitCount);
        Assert.AreEqual(2, sendMessageHitCount);
        Assert.IsTrue(validSendMessageHit);
    }
}
