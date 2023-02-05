using static mark.davison.common.server.abstractions.Authentication.ZenoAuthenticationConstants;

namespace mark.davison.common.server.Authentication;

public static class AuthenticationEndpoints
{
    [ExcludeFromCodeCoverage]
    public static void UseAuthenticationEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
            ZenoRouteNames.LoginRoute,
            async (HttpContext context, CancellationToken cancellationToken) =>
            {
                var zenoAuthOptions = context.RequestServices.GetRequiredService<IOptions<ZenoAuthOptions>>();
                var zenoAuthenticationSession = context.RequestServices.GetRequiredService<IZenoAuthenticationSession>();

                return await Login(context, zenoAuthOptions.Value, zenoAuthenticationSession, cancellationToken);
            });

        endpointRouteBuilder.MapGet(
            ZenoRouteNames.LoginCallbackRoute,
            async (HttpContext context, ILogger<ZenoAuthOptions> logger, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
            {
                var zenoAuthOptions = context.RequestServices.GetRequiredService<IOptions<ZenoAuthOptions>>();
                var zenoAuthenticationSession = context.RequestServices.GetRequiredService<IZenoAuthenticationSession>();

                return await LoginCallback(context, logger, httpClientFactory, zenoAuthOptions.Value, zenoAuthenticationSession, cancellationToken);
            });

        endpointRouteBuilder.MapGet(ZenoRouteNames.LogoutRoute, async (HttpContext context, CancellationToken cancellationToken) =>
        {
            var zenoAuthOptions = context.RequestServices.GetRequiredService<IOptions<ZenoAuthOptions>>();
            var zenoAuthenticationSession = context.RequestServices.GetRequiredService<IZenoAuthenticationSession>();

            return await Logout(context, zenoAuthOptions.Value, zenoAuthenticationSession, cancellationToken);
        });

        endpointRouteBuilder.MapGet(ZenoRouteNames.LogoutCallbackRoute, (HttpContext context, CancellationToken cancellationToken) =>
        {
            var zenoAuthOptions = context.RequestServices.GetRequiredService<IOptions<ZenoAuthOptions>>();

            return LogoutCallback(context, zenoAuthOptions.Value, cancellationToken);
        });

        endpointRouteBuilder.MapGet(ZenoRouteNames.UserRoute, async (HttpContext context, CancellationToken cancellationToken) =>
        {
            var zenoAuthenticationSession = context.RequestServices.GetRequiredService<IZenoAuthenticationSession>();

            return await GetUser(context, zenoAuthenticationSession, cancellationToken);
        });
    }

    public static async Task<RedirectHttpResult> Login(HttpContext context, ZenoAuthOptions zenoAuthOptions, IZenoAuthenticationSession zenoAuthenticationSession, CancellationToken cancellationToken)
    {
        using var mySHA256 = SHA256.Create();
        var verifier = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var challenge = WebEncoders.Base64UrlEncode(mySHA256.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
        var state = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        await zenoAuthenticationSession.LoadSessionAsync(cancellationToken);
        zenoAuthenticationSession.SetString(SessionNames.Verifier, verifier);
        zenoAuthenticationSession.SetString(SessionNames.Challenge, challenge);
        zenoAuthenticationSession.SetString(SessionNames.State, state);
        zenoAuthenticationSession.SetString(SessionNames.RedirectUri, context.Request.Query[ZenoQueryNames.RedirectUri]!);
        zenoAuthenticationSession.Remove(SessionNames.AccessToken);
        zenoAuthenticationSession.Remove(SessionNames.RefreshToken);
        zenoAuthenticationSession.Remove(SessionNames.UserProfile);
        await zenoAuthenticationSession.CommitSessionAsync(cancellationToken);

        var queryParams = new Dictionary<string, string> {
                { OauthParamNames.ClientId,             zenoAuthOptions.ClientId },
                { OauthParamNames.RedirectUri,          zenoAuthOptions.BffOrigin + ZenoRouteNames.LoginCallbackRoute },
                { OauthParamNames.ResponseType,         OauthParams.Code },
                { OauthParamNames.Scope,                zenoAuthOptions.Scope },
                { OauthParamNames.Audience,             zenoAuthOptions.OpenIdConnectConfiguration.Issuer },
                { OauthParamNames.CodeChallengeMethod,  OauthParams.S256ChallengeMethod },
                { OauthParamNames.CodeChallenge,        challenge },
                { OauthParamNames.State,                state },
            };

        return WebUtilities.RedirectPreserveMethod(WebUtilities.CreateQueryUri(zenoAuthOptions.OpenIdConnectConfiguration.AuthorizationEndpoint, queryParams).ToString());
    }

    public static async Task<IResult> LoginCallback(HttpContext context, ILogger logger, IHttpClientFactory httpClientFactory, ZenoAuthOptions zenoAuthOptions, IZenoAuthenticationSession zenoAuthenticationSession, CancellationToken cancellationToken)
    {
        var error = context.Request.Query[OauthQueryNames.Error];
        var error_description = context.Request.Query[OauthQueryNames.ErrorDescription];
        var state = context.Request.Query[OauthQueryNames.State];
        var code = context.Request.Query[OauthQueryNames.Code];

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(error_description))
        {
            var webErrorQueryParams = new Dictionary<string, string>
                {
                    { ZenoQueryNames.Error, error! },
                    { ZenoQueryNames.ErrorDescription, error_description! }
                };
            return WebUtilities.RedirectPreserveMethod(WebUtilities.CreateQueryUri(zenoAuthOptions.WebOrigin + ZenoRouteNames.WebErrorRoute, webErrorQueryParams).ToString());
        }

        await zenoAuthenticationSession.LoadSessionAsync(cancellationToken);
        var sessionState = zenoAuthenticationSession.GetString(SessionNames.State);
        if (sessionState != state)
        {
            var webErrorQueryParams = new Dictionary<string, string>
                {
                    { ZenoQueryNames.Error, ZenoAuthErrors.AuthError },
                    { ZenoQueryNames.ErrorDescription, ZenoAuthErrors.StateMismatch }
            };

            logger.LogError("Auth state mismatch, session state: {0}, url state: {1}", sessionState, state);

            return WebUtilities.RedirectPreserveMethod(WebUtilities.CreateQueryUri(zenoAuthOptions.WebOrigin + ZenoRouteNames.WebErrorRoute, webErrorQueryParams).ToString());
        }

        var redirect = zenoAuthenticationSession.GetString(SessionNames.RedirectUri);

        var queryParams = new Dictionary<string, string> {
                { OauthParamNames.CodeVerifier,     zenoAuthenticationSession.GetString(SessionNames.Verifier)! },
                { OauthParamNames.Code,             code! },
                { OauthParamNames.GrantType,        OauthParams.AuthorizationCodeGrantType },
                { OauthParamNames.ClientId,         zenoAuthOptions.ClientId },
                { OauthParamNames.ClientSecret,     zenoAuthOptions.ClientSecret },
                { OauthParamNames.RedirectUri,      zenoAuthOptions.BffOrigin + ZenoRouteNames.LoginCallbackRoute }
            };
        AuthTokens? tokens;
        UserProfile? userProfile;
        var client = httpClientFactory.CreateClient(AuthClientName);
        var message = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(zenoAuthOptions.OpenIdConnectConfiguration.TokenEndpoint),
            Headers = { { HttpRequestHeader.ContentType.ToString(), WebUtilities.ContentType.FormUrlEncoded } },
            Content = new FormUrlEncodedContent(queryParams)
        };


        using (HttpResponseMessage responseMessage = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                logger.LogError("Retrieving auth tokens returned {0}", responseMessage.StatusCode);
                return Results.BadRequest();
            }

            var tokenString = await responseMessage.Content.ReadAsStringAsync();
            tokens = JsonSerializer.Deserialize<AuthTokens>(tokenString);
            if (tokens == null || string.IsNullOrEmpty(tokens.access_token) || string.IsNullOrEmpty(tokens.refresh_token))
            {
                return Results.BadRequest();
            }
        }

        var userMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(zenoAuthOptions.OpenIdConnectConfiguration.UserInfoEndpoint),
            Headers = { { HttpRequestHeader.Authorization.ToString(), WebUtilities.CreateBearerHeaderValue(tokens.access_token) } }
        };
        using (HttpResponseMessage responseMessage = await client.SendAsync(userMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                return Results.BadRequest();
            }
            var userProfileString = await responseMessage.Content.ReadAsStringAsync();
            userProfile = JsonSerializer.Deserialize<UserProfile>(userProfileString);
            if (userProfile == null || userProfile.sub == Guid.Empty)
            {
                return Results.BadRequest();
            }
        }

        zenoAuthenticationSession.Remove(SessionNames.Verifier);
        zenoAuthenticationSession.Remove(SessionNames.Challenge);
        zenoAuthenticationSession.Remove(SessionNames.State);
        zenoAuthenticationSession.Remove(SessionNames.RedirectUri);
        zenoAuthenticationSession.SetString(SessionNames.AccessToken, tokens.access_token!);
        zenoAuthenticationSession.SetString(SessionNames.RefreshToken, tokens.refresh_token!);
        zenoAuthenticationSession.SetString(SessionNames.UserProfile, JsonSerializer.Serialize(userProfile));

        var customActions = context.RequestServices.GetRequiredService<ICustomZenoAuthenticationActions>();

        var user = await customActions.OnUserAuthenticated(userProfile, zenoAuthenticationSession, cancellationToken);

        if (user != null)
        {
            zenoAuthenticationSession.SetString(SessionNames.User, JsonSerializer.Serialize(user));
        }
        else
        {
            return Results.BadRequest();
        }

        await zenoAuthenticationSession.CommitSessionAsync(cancellationToken);

        return WebUtilities.RedirectPreserveMethod(zenoAuthOptions.WebOrigin + redirect);
    }

    public static async Task<RedirectHttpResult> Logout(HttpContext context, ZenoAuthOptions zenoAuthOptions, IZenoAuthenticationSession zenoAuthenticationSession, CancellationToken cancellationToken)
    {
        var refreshToken = zenoAuthenticationSession.GetString(SessionNames.RefreshToken);
        var access_token = zenoAuthenticationSession.GetString(SessionNames.AccessToken);

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(access_token))
        {
            return (RedirectHttpResult)Results.Redirect(zenoAuthOptions.WebOrigin);
        }

        zenoAuthenticationSession.Clear();
        await zenoAuthenticationSession.CommitSessionAsync(cancellationToken);

        var queryParams = new Dictionary<string, string> {
                { OauthParamNames.ClientId, zenoAuthOptions.ClientId },
                { OauthParamNames.RefreshToken, refreshToken },
                { OauthParamNames.RedirectUri, zenoAuthOptions.BffOrigin + ZenoRouteNames.LogoutCallbackRoute }
            };

        return WebUtilities.RedirectPreserveMethod(WebUtilities.CreateQueryUri(zenoAuthOptions.OpenIdConnectConfiguration.EndSessionEndpoint, queryParams).ToString());
    }

    public static Task<RedirectHttpResult> LogoutCallback(HttpContext context, ZenoAuthOptions zenoAuthOptions, CancellationToken cancellationToken)
    {
        return Task.FromResult(WebUtilities.RedirectPreserveMethod(zenoAuthOptions.WebOrigin));
    }

    public static async Task<IResult> GetUser(HttpContext context, IZenoAuthenticationSession zenoAuthenticationSession, CancellationToken cancellationToken)
    {
        await zenoAuthenticationSession.LoadSessionAsync(cancellationToken);

        var userString = zenoAuthenticationSession.GetString(SessionNames.UserProfile);
        var refreshToken = zenoAuthenticationSession.GetString(SessionNames.RefreshToken);

        if (string.IsNullOrEmpty(userString) || string.IsNullOrEmpty(refreshToken))
        {
            zenoAuthenticationSession.Clear();
            await zenoAuthenticationSession.CommitSessionAsync(cancellationToken);
            return Results.Unauthorized();
        }

        var profile = JsonSerializer.Deserialize<UserProfile>(userString);
        if (profile == null)
        {
            zenoAuthenticationSession.Clear();
            await zenoAuthenticationSession.CommitSessionAsync(cancellationToken);
            return Results.Unauthorized();
        }

        return Results.Ok(profile);
    }
}
