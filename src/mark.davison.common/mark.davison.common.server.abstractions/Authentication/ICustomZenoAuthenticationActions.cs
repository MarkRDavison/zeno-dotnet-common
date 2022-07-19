namespace mark.davison.common.server.abstractions.Authentication;

public interface ICustomZenoAuthenticationActions
{
    Task OnUserAuthenticated(UserProfile userProfile, CancellationToken cancellationToken);
}
