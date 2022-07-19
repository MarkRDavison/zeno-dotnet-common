namespace mark.davison.common.server.Authentication;

public class HydrateAuthenticationFromSessionMiddleware
{
    private readonly RequestDelegate _next;

    public HydrateAuthenticationFromSessionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context, ICurrentUserContext currentUserContext)
    {
        var token = context.Session.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken);
        var userContent = context.Session.GetString(ZenoAuthenticationConstants.SessionNames.User);
        var userProfileContent = context.Session.GetString(ZenoAuthenticationConstants.SessionNames.UserProfile);
        if (!string.IsNullOrEmpty(userContent) && !string.IsNullOrEmpty(userProfileContent) && !string.IsNullOrEmpty(token))
        {
            var user = JsonSerializer.Deserialize<User>(userContent);
            var userProfile = JsonSerializer.Deserialize<UserProfile>(userProfileContent);
            if (user != null && user.Sub != Guid.Empty && userProfile != null && userProfile.sub != Guid.Empty)
            {
                currentUserContext.CurrentUser = user;
                currentUserContext.Token = token;
                context.User =
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[] {
                                    new Claim(nameof(UserProfile.sub), userProfile.sub.ToString()),
                                    new Claim(nameof(UserProfile.email_verified), userProfile.email_verified.ToString()),
                                    new Claim(nameof(UserProfile.name), userProfile.name!),
                                    new Claim(nameof(UserProfile.preferred_username), userProfile.preferred_username!),
                                    new Claim(nameof(UserProfile.given_name), userProfile.given_name!),
                                    new Claim(nameof(UserProfile.family_name), userProfile.family_name!),
                                    new Claim(nameof(UserProfile.email), userProfile.email!)
                            },
                            ZenoAuthenticationConstants.ZenoAuthenticationScheme));
            }
        }
        await _next(context);
    }
}