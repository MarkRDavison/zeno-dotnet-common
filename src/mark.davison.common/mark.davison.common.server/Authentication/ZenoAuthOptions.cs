namespace mark.davison.common.server.Authentication;

[ExcludeFromCodeCoverage]
public class ZenoAuthOptions : IOptions<ZenoAuthOptions>
{
    ZenoAuthOptions IOptions<ZenoAuthOptions>.Value => this;

    public ZenoAuthOptions()
    {
        Scope = string.Empty;
        WebOrigin = string.Empty;
        BffOrigin = string.Empty;
        ClientId = string.Empty;
        ClientSecret = string.Empty;
        OpenIdConnectWellKnownUri = string.Empty;
    }
    public ZenoAuthOptions(
        string scope,
        string webOrigin,
        string bffOrigin,
        string bffClientId,
        string bffClientSecret,
        string openIdConnectWellKnownUri)
    {
        Scope = scope;
        WebOrigin = webOrigin;
        BffOrigin = bffOrigin;
        ClientId = bffClientId;
        ClientSecret = bffClientSecret;
        OpenIdConnectWellKnownUri = openIdConnectWellKnownUri;
    }

    public string Scope { get; set; }
    public string WebOrigin { get; set; }
    public string BffOrigin { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string OpenIdConnectWellKnownUri { get; set; }
    public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; } = new();
}