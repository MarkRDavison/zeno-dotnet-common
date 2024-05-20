namespace mark.davison.common.Identification;

public static class AuthConstants
{
    public const string AuthHttpClient = "Auth";
    public const string ExpiresAtTokenParameter = "expires_at";
    public const string CookiesScheme = "Cookies";
    public const string OidcScheme = "oidc";
    public const string LoginPath = "/auth/login";
    public const string LogoutPath = "/auth/logout";
    public const string LoginCompletePath = "/";
    public const string LogoutCompletePath = "/logout-complete";
    public const string HttpsProto = "https://";
    public const string HttpProto = "http://";
}
