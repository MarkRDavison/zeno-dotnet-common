namespace mark.davison.common.server.abstractions.Authentication;

public interface ICustomZenoAuthenticationActions
{
    Task<User?> OnUserAuthenticated(UserProfile userProfile, IZenoAuthenticationSession session, CancellationToken cancellationToken);
}
