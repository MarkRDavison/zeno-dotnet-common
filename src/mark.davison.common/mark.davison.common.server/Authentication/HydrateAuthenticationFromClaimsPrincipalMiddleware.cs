namespace mark.davison.common.server.Authentication;

public class HydrateAuthenticationFromClaimsPrincipalMiddleware
{
    private readonly RequestDelegate _next;

    public HydrateAuthenticationFromClaimsPrincipalMiddleware(
        RequestDelegate next
    )
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ICurrentUserContext currentUserContext)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            User? user = null;
            if (context.Request.Headers.TryGetValue(ZenoAuthenticationConstants.HeaderNames.User, out var headerUserString))
            {
                user = JsonSerializer.Deserialize<User>(headerUserString!, SerializationHelpers.CreateStandardSerializationOptions());
            }

            if (user == null)
            {
                var subClaim = context.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier);
                if (Guid.TryParse(subClaim?.Value, out var sub))
                {
                    var repository = context.RequestServices.GetRequiredService<IRepository>();
                    await using (repository.BeginTransaction())
                    {
                        user = await repository.GetEntityAsync<User>(_ => _.Sub == sub, CancellationToken.None);
                    }
                }
            }

            if (user != null)
            {
                currentUserContext.CurrentUser = user;
            }

            currentUserContext.Token = string.Empty;
        }

        await _next(context);
    }
}