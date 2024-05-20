namespace mark.davison.common.client.abstractions.Authentication;

public interface IAuthenticationConfig
{
    string LoginEndpoint { get; set; }
    string LogoutEndpoint { get; set; }
    string UserEndpoint { get; set; }
    string HttpClientName { get; set; }
    string BffBase { get; }
    void SetBffBase(string bffBase);
}
