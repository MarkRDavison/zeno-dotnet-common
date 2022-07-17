namespace mark.davison.common.client.abstractions.Authentication;

public interface IAuthenticationContext
{
    Guid UserId { get; set; }
    bool IsAuthenticated { get; set; }
    bool IsAuthenticating { get; set; }
    UserProfile? User { get; set; }

    Task ValidateAuthState();
    Task Login();
    Task Logout();

}
