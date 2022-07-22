namespace mark.davison.common.client.Authentication;

public class AuthenticationConfig : IAuthenticationConfig
{
    public string LoginEndpoint { get; set; } = string.Empty;
    public string LogoutEndpoint { get; set; } = string.Empty;
    public string UserEndpoint { get; set; } = string.Empty;
    public string BffBase { get; private set; } = string.Empty;
    public void SetBffBase(string bffBase)
    {
        BffBase = bffBase.TrimEnd('/');
        LoginEndpoint = BffBase + "/auth/login";
        LogoutEndpoint = BffBase + "/auth/logout";
        UserEndpoint = BffBase + "/auth/user";
    }
}
