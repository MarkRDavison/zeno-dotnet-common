namespace mark.davison.common.server.Authentication;

public sealed class PopulateUserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<ClaimsAppSettings> _claimsSettings;

    public PopulateUserContextMiddleware(
        RequestDelegate next,
        IOptions<ClaimsAppSettings> claimsSettings)
    {
        _next = next;
        _claimsSettings = claimsSettings;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated ?? false)
        {
            var user = context.User.ExtractUser(_claimsSettings.Value);

            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Could not extract user from claims");
                return;
            }

            var userContext = context.RequestServices.GetRequiredService<ICurrentUserContext>();

            userContext.CurrentUser = user;
        }

        await _next(context);
    }
}