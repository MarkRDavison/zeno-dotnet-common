namespace mark.davison.common.server.abstractions.Authentication;

public static class ZenoAuthenticationConstants
{

    public const string ProxyHttpClientName = "ZENO_HTTP_PROXY_AUTH_NAME";
    public const string AuthClientName = "ZENO_HTTP_CLIENT_AUTH_NAME";
    public const string ZenoAuthenticationScheme = "ZenoAuth";

    public static class ZenoAuthErrors
    {
        public const string AuthError = "auth error";
        public const string StateMismatch = "Auth states do not match";
    }

    public static class ZenoRouteNames
    {
        public const string AuthResource = "auth";
        public const string User = "user";
        public const string Login = "login";
        public const string LoginCallback = "login-callback";
        public const string Logout = "logout";
        public const string LogoutCallback = "logout-callback";
        public const string WebError = "error";

        public const string UserRoute = $"/{AuthResource}/{User}";
        public const string LoginRoute = $"/{AuthResource}/{Login}";
        public const string LoginCallbackRoute = $"/{AuthResource}/{LoginCallback}";
        public const string LogoutRoute = $"/{AuthResource}/{Logout}";
        public const string LogoutCallbackRoute = $"/{AuthResource}/{LogoutCallback}";
        public const string WebErrorRoute = $"/{WebError}";
    }

    public static class ZenoQueryNames
    {
        public const string RedirectUri = "redirect_uri";
        public const string Error = "error";
        public const string ErrorDescription = "error_description";
    }

    public static class OauthParamNames
    {
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string RedirectUri = "redirect_uri";
        public const string ResponseType = "response_type";
        public const string Scope = "scope";
        public const string Audience = "audience";
        public const string CodeChallengeMethod = "code_challenge_method";
        public const string CodeChallenge = "code_challenge";
        public const string CodeVerifier = "code_verifier";
        public const string Code = "code";
        public const string GrantType = "grant_type";
        public const string State = "state";
        public const string RefreshToken = "refresh_token";
    }

    public static class OauthQueryNames
    {
        public const string Error = "error";
        public const string ErrorDescription = "error_description";
        public const string State = "state";
        public const string Code = "code";
    }

    public static class OauthParams
    {
        public const string Code = "code";
        public const string S256ChallengeMethod = "S256";
        public const string AuthorizationCodeGrantType = "authorization_code";
        public const string ClientCredentialsGrantType = "client_credentials";
    }

    public static class SessionNames
    {
        public const string Verifier = "verifier";
        public const string Challenge = "challenge";
        public const string State = "state";
        public const string RedirectUri = "redirect_uri";
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string UserProfile = "user_profile";
        public const string User = "user";
    }

    public static class HeaderNames
    {
        public const string User = "user";
    }
}