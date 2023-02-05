namespace mark.davison.common.server.Authentication;

public class StandardZenoAuthenticationActions : ICustomZenoAuthenticationActions
{
    private readonly IHttpRepository _httpRepository;
    private readonly IDateService _dateService;

    public StandardZenoAuthenticationActions(
        IHttpRepository httpRepository,
        IDateService dateService
    )
    {
        _httpRepository = httpRepository;
        _dateService = dateService;
    }

    private Task<User?> GetUser(Guid sub, CancellationToken cancellationToken)
    {
        return _httpRepository.GetEntityAsync<User>(
            new QueryParameters { { nameof(User.Sub), sub.ToString() } },
            HeaderParameters.None,
            cancellationToken);
    }

    private Task<User?> UpsertUser(UserProfile userProfile, string token, CancellationToken cancellationToken)
    {
        return _httpRepository.UpsertEntityAsync(
            new User
            {
                Id = Guid.NewGuid(),
                Sub = userProfile.sub,
                Admin = false,
                Created = _dateService.Now,
                Email = userProfile.email ?? string.Empty,
                First = userProfile.given_name ?? string.Empty,
                Last = userProfile.family_name ?? string.Empty,
                Username = userProfile.preferred_username ?? string.Empty,
                LastModified = _dateService.Now
            },
            HeaderParameters.Auth(token, null),
            cancellationToken);
    }

    public async Task OnUserAuthenticated(UserProfile userProfile, IZenoAuthenticationSession zenoAuthenticationSession, CancellationToken cancellationToken)
    {
        var token = zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken);
        var user = await GetUser(userProfile.sub, cancellationToken);

        if (user == null && !string.IsNullOrEmpty(token))
        {
            user = await UpsertUser(userProfile, token, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("UpsertUser did not succeed");
            }
        }

        if (user != null)
        {
            zenoAuthenticationSession.SetString(ZenoAuthenticationConstants.SessionNames.User, JsonSerializer.Serialize(user));
        }
    }
}
