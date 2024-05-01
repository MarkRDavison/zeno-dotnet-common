namespace mark.davison.common.server.Authentication;

public class HydrateAuthenticationFromSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IZenoAuthenticationSession _zenoAuthenticationSession;
    private readonly IDateService _dateService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ZenoAuthOptions _zenoAuthOptions;
    private readonly ILogger<HydrateAuthenticationFromSessionMiddleware> _logger;

    public HydrateAuthenticationFromSessionMiddleware(
        RequestDelegate next,
        IZenoAuthenticationSession zenoAuthenticationSession,
        IDateService dateService,
        IHttpClientFactory httpClientFactory,
        IOptions<ZenoAuthOptions> zenoAuthOptions,
        ILogger<HydrateAuthenticationFromSessionMiddleware> logger)
    {
        _next = next;
        _zenoAuthenticationSession = zenoAuthenticationSession;
        _dateService = dateService;
        _httpClientFactory = httpClientFactory;
        _zenoAuthOptions = zenoAuthOptions.Value;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, ICurrentUserContext currentUserContext)
    {
        await _zenoAuthenticationSession.LoadSessionAsync(CancellationToken.None);

        var accessToken = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken);
        var refreshToken = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.RefreshToken);
        var userContent = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.User);
        var userProfileContent = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.UserProfile);
        if (!string.IsNullOrEmpty(userContent) && !string.IsNullOrEmpty(userProfileContent) && !string.IsNullOrEmpty(accessToken))
        {
            User? user = null;
            UserProfile? userProfile = null;
            try
            {
                user = JsonSerializer.Deserialize<User>(userContent);
                userProfile = JsonSerializer.Deserialize<UserProfile>(userProfileContent);
            }
            catch { }

            if (user != null && userProfile != null)
            {
                var tokens = await AuthenticationEndpoints.EnsureValidAccessToken(
                    accessToken,
                    refreshToken,
                    _dateService,
                    _httpClientFactory,
                    _zenoAuthOptions,
                    _zenoAuthenticationSession,
                    _logger,
                    CancellationToken.None);

                if (tokens.Valid)
                {
                    accessToken = tokens.access_token;
                    refreshToken = tokens.refresh_token;
                    _logger.LogInformation("Validated auth tokens");
                }
                else
                {
                    accessToken = string.Empty;
                    refreshToken = string.Empty;
                    _logger.LogWarning("Failed to validate auth tokens");
                }
            }

            if (user != null &&
                user.Sub != Guid.Empty &&
                userProfile != null &&
                userProfile.sub != Guid.Empty &&
                !string.IsNullOrEmpty(accessToken) &&
                !string.IsNullOrEmpty(refreshToken))
            {
                currentUserContext.CurrentUser = user;
                currentUserContext.Token = accessToken;
                context.User =
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim(nameof(UserProfile.sub), userProfile.sub.ToString()),
                                new Claim(nameof(UserProfile.email_verified), userProfile.email_verified.ToString()),
                                new Claim(nameof(UserProfile.name), userProfile.name!),
                                new Claim(nameof(UserProfile.preferred_username), userProfile.preferred_username!),
                                new Claim(nameof(UserProfile.given_name), userProfile.given_name!),
                                new Claim(nameof(UserProfile.family_name), userProfile.family_name!),
                                new Claim(nameof(UserProfile.email), userProfile.email!)
                            ],
                            ZenoAuthenticationConstants.ZenoAuthenticationScheme));
            }
        }

        await _next(context);
    }
}