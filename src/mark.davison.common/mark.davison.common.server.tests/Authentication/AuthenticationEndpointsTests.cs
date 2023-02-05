using Microsoft.AspNetCore.Http.HttpResults;
using static mark.davison.common.server.abstractions.Authentication.ZenoAuthenticationConstants;

namespace mark.davison.common.server.tests.Authentication;

[TestClass]
public class AuthenticationEndpointsTests
{
    private readonly Mock<ILogger<ZenoAuthOptions>> _logger;
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IServiceProvider> _requestServicesServiceProvider;
    private readonly Mock<IZenoAuthenticationSession> _zenoAuthenticationSession;
    private readonly Mock<ICustomZenoAuthenticationActions> _customZenoAuthenticationActions;

    private readonly ZenoAuthOptions _zenoAuthOptions;
    private readonly HttpContext _httpContext;
    private readonly MockHttpMessageHandler _httpMessageHandler;

    public AuthenticationEndpointsTests()
    {
        _logger = new();
        _httpClientFactory = new(MockBehavior.Strict);
        _httpContextAccessor = new(MockBehavior.Strict);
        _serviceProvider = new(MockBehavior.Strict);
        _requestServicesServiceProvider = new(MockBehavior.Strict);
        _customZenoAuthenticationActions = new(MockBehavior.Strict);
        _zenoAuthenticationSession = new(MockBehavior.Strict);
        _zenoAuthOptions = new()
        {
            WebOrigin = "https://web-origin/",
            BffOrigin = "https://bff-origin/",
            OpenIdConnectConfiguration = new()
            {
                EndSessionEndpoint = "https://end-session-endpoint/",
                AuthorizationEndpoint = "https://authorization-endpoint/",
                TokenEndpoint = "https://token-endpoint/",
                UserInfoEndpoint = "https://user-info-endpoint/"
            }
        };
        _httpContext = new DefaultHttpContext
        {
            RequestServices = _requestServicesServiceProvider.Object
        };
        _httpMessageHandler = new();
        _httpContextAccessor.Setup(_ => _.HttpContext).Returns(_httpContext);
    }

    [TestMethod]
    public async Task Login_UpdatesSession_AsExpected()
    {
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.Verifier, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.Challenge, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.State, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.RedirectUri, It.IsAny<string>()))
            .Verifiable();

        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.AccessToken))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.RefreshToken))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.UserProfile))
            .Verifiable();

        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await AuthenticationEndpoints.Login(_httpContext, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        _zenoAuthenticationSession.VerifyAll();
    }

    [TestMethod]
    public async Task Login_RedirectsToAuthorizationEndpoint()
    {
        var context = new DefaultHttpContext
        {
            RequestServices = _requestServicesServiceProvider.Object
        };

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.Verifier, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.Challenge, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.State, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.RedirectUri, It.IsAny<string>()));

        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.AccessToken));
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.RefreshToken));
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.UserProfile));

        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await AuthenticationEndpoints.Login(_httpContext, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);

        Assert.IsTrue(result.Url.Contains(_zenoAuthOptions.OpenIdConnectConfiguration.AuthorizationEndpoint));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.ClientId));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.ResponseType));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.Scope));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.Audience));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.State));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.CodeChallengeMethod));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.CodeChallenge));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(OauthParamNames.RedirectUri)));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(ZenoRouteNames.LoginCallbackRoute)));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(_zenoAuthOptions.BffOrigin)));
    }

    [DataTestMethod]
    [DataRow("error", "")]
    [DataRow("", "description")]
    [DataRow("error", "description")]
    public async Task LoginCallback_WhereErrorInQuery_ReturnsRedirectToErrorRoute(string error, string errorDescription)
    {
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.Error, error },
            { OauthQueryNames.ErrorDescription, errorDescription }
        });

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Url.Contains(_zenoAuthOptions.WebOrigin));
        Assert.IsTrue(result.Url.Contains(ZenoRouteNames.WebErrorRoute));
    }

    [TestMethod]
    public async Task LoginCallback_WhereStateMismatch_ReturnsRedirectToErrorRoute()
    {
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, "some-random-string" }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns("some-different-random-string")
            .Verifiable();

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        _zenoAuthenticationSession
            .Verify(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        _zenoAuthenticationSession
            .Verify(_ => _.GetString(SessionNames.State),
                Times.Once);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Url.Contains(_zenoAuthOptions.WebOrigin));
        Assert.IsTrue(result.Url.Contains(ZenoRouteNames.WebErrorRoute));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(ZenoAuthErrors.StateMismatch)));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(ZenoAuthErrors.AuthError)));
    }

    [TestMethod]
    public async Task LoginCallback_WhereAuthTokensNull_ReturnsBadRequest()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };
        };

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task LoginCallback_WhereAuthTokensRequestFails_ReturnsBadRequest()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BadRequest));
    }

    [TestMethod]
    public async Task LoginCallback_WhereUserInfoRequestFails_ReturnsBadRequest()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            if (_.RequestUri!.ToString().StartsWith(_zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new AuthTokens
                    {
                        access_token = "access_token",
                        refresh_token = "refresh_token"
                    }))
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BadRequest));
    }

    [TestMethod]
    public async Task LoginCallback_WhereUserInfoNull_ReturnsBadRequest()
    {
        var state = "state-string";
        var redirect = "path/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            if (_.RequestUri!.ToString().StartsWith(_zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new AuthTokens
                    {
                        access_token = "access_token",
                        refresh_token = "refresh_token"
                    }))
                };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("null")
                };
            }
        };

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BadRequest));
    }

    [TestMethod]
    public async Task LoginCallback_WhereSuccess_UpdatesSession()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);
        _serviceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Verifier))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Challenge))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.State))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.RedirectUri))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.AccessToken, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.RefreshToken, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.UserProfile, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.User, It.IsAny<string>()));

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            if (_.RequestUri!.ToString().StartsWith(_zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new AuthTokens
                    {
                        access_token = "access_token",
                        refresh_token = "refresh_token"
                    }))
                };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new UserProfile
                    {
                        sub = Guid.NewGuid()
                    }))
                };
            }
        };

        _customZenoAuthenticationActions
            .Setup(_ => _.OnUserAuthenticated(
                It.IsAny<UserProfile>(),
                It.IsAny<IZenoAuthenticationSession>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(_zenoAuthOptions.WebOrigin + redirect, result.Url);

        _zenoAuthenticationSession.VerifyAll();
    }

    [TestMethod]
    public async Task LoginCallback_WhereCustomActionsReturnsNullUser_ReturnsBadRequest()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);
        _serviceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Verifier))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Challenge))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.State))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.RedirectUri))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.AccessToken, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.RefreshToken, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.UserProfile, It.IsAny<string>()))
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.User, It.IsAny<string>()));

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            if (_.RequestUri!.ToString().StartsWith(_zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new AuthTokens
                    {
                        access_token = "access_token",
                        refresh_token = "refresh_token"
                    }))
                };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new UserProfile
                    {
                        sub = Guid.NewGuid()
                    }))
                };
            }
        };

        _customZenoAuthenticationActions
            .Setup(_ => _.OnUserAuthenticated(
                It.IsAny<UserProfile>(),
                It.IsAny<IZenoAuthenticationSession>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BadRequest));
    }

    [TestMethod]
    public async Task LoginCallback_WhereSuccess_CallsOnUserAuthenticated()
    {
        var state = "state-string";
        var redirect = "/";
        _httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
            { OauthQueryNames.State, state }
        });

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);
        _serviceProvider
            .Setup(_ => _.GetService(typeof(ICustomZenoAuthenticationActions)))
            .Returns(() => _customZenoAuthenticationActions.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.State))
            .Returns(state);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RedirectUri))
            .Returns(redirect);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.Verifier))
            .Returns("verifier");

        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Verifier));
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.Challenge));
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.State));
        _zenoAuthenticationSession
            .Setup(_ => _.Remove(SessionNames.RedirectUri));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.AccessToken, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.RefreshToken, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.UserProfile, It.IsAny<string>()));
        _zenoAuthenticationSession
            .Setup(_ => _.SetString(SessionNames.User, It.IsAny<string>()));

        _customZenoAuthenticationActions
            .Setup(_ => _.OnUserAuthenticated(It.IsAny<UserProfile>(), _zenoAuthenticationSession.Object, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<User?>(new User()))
            .Verifiable();

        _httpClientFactory
            .Setup(_ => _.CreateClient(AuthClientName))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.SendAsyncFunc = _ =>
        {
            if (_.RequestUri!.ToString().StartsWith(_zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new AuthTokens
                    {
                        access_token = "access_token",
                        refresh_token = "refresh_token"
                    }))
                };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new UserProfile
                    {
                        sub = Guid.NewGuid()
                    }))
                };
            }
        };

        await AuthenticationEndpoints.LoginCallback(_httpContext, _logger.Object, _httpClientFactory.Object, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None);

        _customZenoAuthenticationActions
            .Verify(_ => _.OnUserAuthenticated(It.IsAny<UserProfile>(), _zenoAuthenticationSession.Object, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [DataTestMethod]
    [DataRow("", "")]
    [DataRow("access", "")]
    [DataRow("", "refresh")]
    public async Task Logout_RedirectsToWebOrigin_WhenSessionIsInvalid(string refreshToken, string accessToken)
    {
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.AccessToken))
            .Returns(accessToken);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RefreshToken))
            .Returns(refreshToken);

        var result = await AuthenticationEndpoints.Logout(_httpContext, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(_zenoAuthOptions.WebOrigin, result.Url);
    }

    [TestMethod]
    public async Task Logout_WhereSessionValid_ClearsSession()
    {
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.AccessToken))
            .Returns("accessToken");
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RefreshToken))
            .Returns("refreshToken");

        _zenoAuthenticationSession
            .Setup(_ => _.Clear())
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(CancellationToken.None))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await AuthenticationEndpoints.Logout(_httpContext, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        _zenoAuthenticationSession
            .Verify(_ => _.Clear(),
                Times.Once);
        _zenoAuthenticationSession
            .Verify(_ => _.CommitSessionAsync(CancellationToken.None),
                Times.Once);
    }

    [TestMethod]
    public async Task Logout_WhereSessionValid_RedirectsToLogoutUri()
    {
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.AccessToken))
            .Returns("accessToken");
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RefreshToken))
            .Returns("refreshToken");

        _zenoAuthenticationSession
            .Setup(_ => _.Clear());
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(CancellationToken.None))
            .Returns(Task.CompletedTask);

        var result = await AuthenticationEndpoints.Logout(_httpContext, _zenoAuthOptions, _zenoAuthenticationSession.Object, CancellationToken.None) as RedirectHttpResult;

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Url.Contains(_zenoAuthOptions.OpenIdConnectConfiguration.EndSessionEndpoint));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.ClientId));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.RefreshToken));
        Assert.IsTrue(result.Url.Contains(OauthParamNames.RedirectUri));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(ZenoRouteNames.LogoutCallbackRoute)));
        Assert.IsTrue(result.Url.Contains(HttpUtility.UrlEncode(_zenoAuthOptions.BffOrigin)));
    }

    [TestMethod]
    public async Task LogoutCallback_CreatesRedirectToWebOrigin()
    {
        var result = await AuthenticationEndpoints.LogoutCallback(_httpContext, _zenoAuthOptions, CancellationToken.None) as RedirectHttpResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(_zenoAuthOptions.WebOrigin, result.Url);
    }

    [DataTestMethod]
    [DataRow("", "token")]
    [DataRow("{}", "")]
    [DataRow("", "")]
    [DataRow("null", "token")]
    public async Task GetUser_WhereSessionNotValid_ClearsSession_ReturnsUnauthorized(string userProfile, string token)
    {
        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.UserProfile))
            .Returns(userProfile);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RefreshToken))
            .Returns(token);

        _zenoAuthenticationSession
            .Setup(_ => _.Clear())
            .Verifiable();
        _zenoAuthenticationSession
            .Setup(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await AuthenticationEndpoints.GetUser(_httpContext, _zenoAuthenticationSession.Object, CancellationToken.None);

        _zenoAuthenticationSession
            .Verify(_ => _.Clear(),
                Times.Once);
        _zenoAuthenticationSession
            .Verify(_ => _.CommitSessionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedHttpResult));
    }

    [TestMethod]
    public async Task GetUser_WhereSessionValid_ClearsSession_ReturnsProfile()
    {
        var userProfile = new UserProfile();

        _requestServicesServiceProvider
            .Setup(_ => _.GetService(typeof(IZenoAuthenticationSession)))
            .Returns(() => _zenoAuthenticationSession.Object);

        _zenoAuthenticationSession
            .Setup(_ => _.LoadSessionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.UserProfile))
            .Returns(JsonSerializer.Serialize(userProfile));
        _zenoAuthenticationSession
            .Setup(_ => _.GetString(SessionNames.RefreshToken))
            .Returns("token");

        var result = await AuthenticationEndpoints.GetUser(_httpContext, _zenoAuthenticationSession.Object, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(Ok<UserProfile>));
        var okObjectResult = (Ok<UserProfile>)result;

        Assert.AreEqual(200, okObjectResult.StatusCode);
        Assert.IsInstanceOfType(userProfile, typeof(UserProfile));
    }
}
