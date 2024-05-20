namespace mark.davison.common.server.Authentication;

public class CheckAccessTokenValidityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<AuthAppSettings> _authSettings;
    private readonly HttpClient _client;

    public CheckAccessTokenValidityMiddleware(
        RequestDelegate next,
        IOptions<AuthAppSettings> authSettings,
        IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _authSettings = authSettings;
        _client = httpClientFactory.CreateClient(AuthConstants.AuthHttpClient);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var expireAt = await context.GetTokenAsync(AuthConstants.ExpiresAtTokenParameter);
        if (expireAt != null)
        {
            if (DateTime.TryParse(expireAt, null, DateTimeStyles.RoundtripKind, out var dateExpireAt))
            {
                if ((dateExpireAt - DateTime.Now).TotalMinutes < 2)
                {
                    bool tokenRefreshFailure = true;
                    // TODO: Cache token endpoint
                    var discoveryResponse = await _client.GetDiscoveryDocumentAsync(_authSettings.Value.AUTHORITY);
                    if (!string.IsNullOrWhiteSpace(discoveryResponse.TokenEndpoint))
                    {
                        var tokenClient = new TokenClient(_client, new TokenClientOptions
                        {
                            ClientId = _authSettings.Value.CLIENT_ID,
                            ClientSecret = _authSettings.Value.CLIENT_SECRET,
                            Address = discoveryResponse.TokenEndpoint
                        });

                        var refreshToken = await context.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            var tokenResult = await tokenClient.RequestRefreshTokenAsync(refreshToken);

                            if (!tokenResult.IsError &&
                                !string.IsNullOrEmpty(tokenResult.IdentityToken) &&
                                !string.IsNullOrEmpty(tokenResult.AccessToken) &&
                                !string.IsNullOrEmpty(tokenResult.RefreshToken))
                            {
                                var tokens = new List<AuthenticationToken>
                                {
                                    new AuthenticationToken {
                                        Name = OpenIdConnectParameterNames.IdToken,
                                        Value = tokenResult.IdentityToken
                                    },
                                    new AuthenticationToken
                                    {
                                        Name = OpenIdConnectParameterNames.AccessToken,
                                        Value =  tokenResult.AccessToken
                                    },
                                    new AuthenticationToken
                                    {
                                        Name = OpenIdConnectParameterNames.RefreshToken,
                                        Value = tokenResult.RefreshToken
                                    }
                                };
                                var expiresAt = DateTime.Now + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                                tokens.Add(new AuthenticationToken
                                {
                                    Name = AuthConstants.ExpiresAtTokenParameter,
                                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                                });
                                var info = await context.AuthenticateAsync(AuthConstants.CookiesScheme);

                                if (info != null &&
                                    info.Properties != null &&
                                    info.Principal != null)
                                {
                                    info.Properties.StoreTokens(tokens);
                                    await context.SignInAsync(AuthConstants.CookiesScheme, info.Principal, info.Properties);
                                    tokenRefreshFailure = false;
                                }
                            }
                        }
                    }

                    if (tokenRefreshFailure)
                    {
                        await context.SignOutAsync(AuthConstants.CookiesScheme);
                        await context.SignOutAsync(AuthConstants.OidcScheme);

                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return;
                    }
                }
            }
        }

        var accessToken = await context.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        }

        await _next.Invoke(context);
    }
}
