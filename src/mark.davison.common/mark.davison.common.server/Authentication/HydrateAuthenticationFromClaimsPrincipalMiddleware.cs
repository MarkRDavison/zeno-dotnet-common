namespace mark.davison.common.server.Authentication;

public class HydrateAuthenticationFromClaimsPrincipalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRepository _repository;

    public HydrateAuthenticationFromClaimsPrincipalMiddleware(
        RequestDelegate next,
        IRepository repository)
    {
        _next = next;
        _repository = repository;
    }

    public async Task Invoke(HttpContext context, ICurrentUserContext currentUserContext)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            User? user = null;
            if (context.Request.Headers.TryGetValue(ZenoAuthenticationConstants.HeaderNames.User, out var headerUserString))
            {
                user = JsonSerializer.Deserialize<User>(headerUserString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (user == null)
            {
                var subClaim = context.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier);
                if (Guid.TryParse(subClaim?.Value, out var sub))
                {
                    user = await _repository.GetEntityAsync<User>(_ => _.Sub == sub, CancellationToken.None);
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