namespace mark.davison.common.client.desktop.components.Services;

public sealed class DesktopAuthenticationService : IDesktopAuthenticationService
{
    private readonly string _loginInfoFilename = "login.json";
    private OidcClient? _oidcClient;
    private string _identityToken = string.Empty;
    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;
    private string _username = string.Empty;
    private readonly IOptions<OdicClientSettings> _authSettings;
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;

    public DesktopAuthenticationService(
        IOptions<OdicClientSettings> authSettings,
        ICommonApplicationNotificationService commonApplicationNotificationService)
    {
        _authSettings = authSettings;
        _commonApplicationNotificationService = commonApplicationNotificationService;
    }

    public bool IsAuthenticated =>
        _oidcClient != null &&
        !string.IsNullOrEmpty(_identityToken) &&
        !string.IsNullOrEmpty(_accessToken) &&
        !string.IsNullOrEmpty(_refreshToken) &&
        !string.IsNullOrEmpty(_username);

    public string Username => _username;

    private void SetLoginResult(LoginResult result)
    {
        _identityToken = result.IdentityToken ?? string.Empty;
        _accessToken = result.AccessToken ?? string.Empty;
        _refreshToken = result.RefreshToken ?? string.Empty;
        _username = result.User.Claims.First(_ => _.Type == "preferred_username").Value.ToString();
    }

    public async Task<(bool, string)> LoginAsync(CancellationToken cancellationToken)
    {
        var browser = new SystemBrowser();
        string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

        var options = new OidcClientOptions
        {
            Authority = _authSettings.Value.Authority,
            ClientId = _authSettings.Value.ClientId,
            RedirectUri = redirectUri,
            Scope = _authSettings.Value.Scope,
            FilterClaims = false,
            Browser = browser,
            LoadProfile = true,
            DisablePushedAuthorization = true,
            RefreshTokenInnerHttpHandler = new HttpClientHandler()
        };

        _oidcClient = new OidcClient(options);
        var result = await _oidcClient.LoginAsync(new LoginRequest());

        if (result is null || result.IsError)
        {
            return (false, result?.Error ?? "An error occured logging in");
        }

        SetLoginResult(result);

        _commonApplicationNotificationService.NotifyAuthenticationStateChanged();

        PersistLogin();

        return (true, "Success");
    }

    public async Task<(bool, string)> LoginFromPersisted()
    {
        if (string.IsNullOrEmpty(_authSettings.Value.PersistenceLocation))
        {
            return (false, "Not configured to persist login info");
        }

        if (!Directory.Exists(_authSettings.Value.PersistenceLocation))
        {
            return (false, "Login info directory does not exist");
        }

        var path = Path.Combine(_authSettings.Value.PersistenceLocation, _loginInfoFilename);

        if (!File.Exists(path))
        {
            return (false, "Login info file does not exist");
        }


        var text = File.ReadAllText(path);

        var auth = JsonSerializer.Deserialize<PersistedAuthentication>(text);

        if (auth is null || string.IsNullOrEmpty(auth.RefreshToken))
        {
            return (false, "Failed to extract user info from persisted file");
        }

        var options = new OidcClientOptions
        {
            Authority = _authSettings.Value.Authority,
            ClientId = _authSettings.Value.ClientId,
            Scope = _authSettings.Value.Scope,
            FilterClaims = false,
            LoadProfile = true,
            DisablePushedAuthorization = true,
            RefreshTokenInnerHttpHandler = new HttpClientHandler()
        };

        _oidcClient = new OidcClient(options);

        var result = await _oidcClient.RefreshTokenAsync(auth.RefreshToken);

        if (result is null || result.IsError)
        {
            return (false, result?.Error ?? "An error occured logging in");
        }

        _identityToken = result.IdentityToken ?? string.Empty;
        _accessToken = result.AccessToken ?? string.Empty;
        _refreshToken = result.RefreshToken ?? string.Empty;

        var userInfo = await _oidcClient.GetUserInfoAsync(_accessToken);
        _username = userInfo.Claims.First(_ => _.Type == "preferred_username").Value.ToString();

        _commonApplicationNotificationService.NotifyAuthenticationStateChanged();

        return (true, "Success");
    }

    public void PersistLogin()
    {
        if (string.IsNullOrEmpty(_authSettings.Value.PersistenceLocation) ||
            string.IsNullOrEmpty(_refreshToken) ||
            string.IsNullOrEmpty(_username))
        {
            return;
        }

        if (!Directory.Exists(_authSettings.Value.PersistenceLocation))
        {
            Directory.CreateDirectory(_authSettings.Value.PersistenceLocation);
        }

        var path = Path.Combine(_authSettings.Value.PersistenceLocation, _loginInfoFilename);

        var auth = new PersistedAuthentication
        {
            RefreshToken = _refreshToken,
            Username = _username
        };

        File.WriteAllText(path, JsonSerializer.Serialize(auth, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public void Logout()
    {
        _identityToken = string.Empty;
        _accessToken = string.Empty;
        _refreshToken = string.Empty;
        _username = string.Empty;

        var path = Path.Combine(_authSettings.Value.PersistenceLocation, _loginInfoFilename);

        File.Delete(path);

        _commonApplicationNotificationService.NotifyAuthenticationStateChanged();
    }
}
