namespace mark.davison.common.server;

public static class IEndpointRouteBuilderExtensions
{

    public static IEndpointRouteBuilder UseApiProxy(
        this IEndpointRouteBuilder endpoints,
        string apiEndpoint)
    {
        endpoints.Map("/api/{**catchall}", async (
            HttpContext context,
            [FromServices] IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) =>
        {
            var access_token = await context.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (string.IsNullOrEmpty(access_token))
            {
                return Results.Unauthorized();
            }

            var client = httpClientFactory.CreateClient("ApiProxy");
            var request = new HttpRequestMessage(
                new HttpMethod(context.Request.Method),
                $"{apiEndpoint.TrimEnd('/')}{context.Request.Path}{context.Request.QueryString}")
            {
                Content = new StreamContent(context.Request.Body)
            };

            request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, $"Bearer {access_token}");

            var response = await client.SendAsync(request, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Results.Text(content);
            }

            return Results.BadRequest(new Response
            {
                Errors = ["BAD_REQUEST", $"{response.StatusCode}", content]
            });
        })
        .RequireAuthorization();
        return endpoints;
    }

    public static IEndpointRouteBuilder UseAuthEndpoints(
        this IEndpointRouteBuilder endpoints,
        string webOrigin)
    {
        endpoints
            .MapGet("/auth/user", (HttpContext context, [FromServices] IOptions<ClaimsAppSettings> claimsSettings) =>
            {
                if (context.User?.Identity?.IsAuthenticated ?? false)
                {
                    if (context.User.ExtractUser(claimsSettings.Value) is User user)
                    {
                        return Results.Ok(new UserProfile
                        {
                            // TODO: Maybe add admin?
                            sub = user.Sub,
                            email = user.Email,
                            family_name = user.Last,
                            given_name = user.First,
                            name = $"{user.First} + {user.Last}",
                            preferred_username = user.Username
                        });
                    }
                }

                return Results.Unauthorized();
            })
            .AllowAnonymous();

        endpoints
            .MapGet(AuthConstants.LoginPath, async (HttpContext context, [FromQuery(Name = "returnUrl")] string? returnUrl) =>
            {
                var prop = new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? webOrigin.TrimEnd('/') + AuthConstants.LoginCompletePath
                };

                await context.ChallengeAsync(prop);
            })
            .AllowAnonymous();

        endpoints
            .MapGet(AuthConstants.LogoutPath, async (HttpContext context) =>
            {
                await context.SignOutAsync(AuthConstants.CookiesScheme);
                var prop = new AuthenticationProperties
                {
                    RedirectUri = webOrigin.TrimEnd('/') + AuthConstants.LogoutCompletePath
                };
                await context.SignOutAsync(AuthConstants.OidcScheme, prop);
            })
            .RequireAuthorization();

        return endpoints;
    }
}
