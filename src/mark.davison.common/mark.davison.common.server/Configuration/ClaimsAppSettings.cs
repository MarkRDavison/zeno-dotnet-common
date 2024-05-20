namespace mark.davison.common.server.Configuration;

public sealed class ClaimsAppSettings : IAppSettings
{
    public string SECTION => "CLAIMS";

    public string OIDC_ID_ATTRIBUTE { get; set; } = string.Empty;
    public string OIDC_EMAIL_ATTRIBUTE { get; set; } = string.Empty;
    public string OIDC_FIRST_NAME_ATTRIBUTE { get; set; } = string.Empty;
    public string OIDC_LAST_NAME_ATTRIBUTE { get; set; } = string.Empty;
    public string OIDC_USERNAME_ATTRIBUTE { get; set; } = string.Empty;
    public string OIDC_ADMIN_ATTRIBUTE { get; set; } = string.Empty;
}
