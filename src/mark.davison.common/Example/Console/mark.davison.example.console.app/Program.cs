using IdentityModel.Client;
using mark.davison.common.client.Repository;
using mark.davison.example.console.app;
using Microsoft.Extensions.Logging;

// TODO: To config - but its all public so its fine :)
const string Authority = "https://keycloak.markdavison.kiwi/auth/realms/markdavison.kiwi";
const string ClientId = "zeno-example-public";
const string Scope = "openid profile email offline_access zeno-example-public";
const string ApiRppt = "https://localhost:50000";

Console.WriteLine("+-----------------------+");
Console.WriteLine("|  Sign in with OIDC    |");
Console.WriteLine("+-----------------------+");
Console.WriteLine("");
Console.WriteLine("Press any key to sign in...");
Console.ReadKey();



// create a redirect URI using an available port on the loopback address.
// requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
var browser = new SystemBrowser();
string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");


var options = new OidcClientOptions
{
    Authority = Authority,
    ClientId = ClientId,
    RedirectUri = redirectUri,
    Scope = Scope,
    FilterClaims = false,
    Browser = browser,
    LoadProfile = true,
    DisablePushedAuthorization = true,
    RefreshTokenInnerHttpHandler = new HttpClientHandler()
};


OidcClient _oidcClient = new OidcClient(options);
var result = await _oidcClient.LoginAsync(new LoginRequest());

if (result.IsError)
{
    Console.WriteLine("\n\nError:\n{0}", result.Error);
    Console.ReadKey();
    return;
}

var factory = LoggerFactory.Create(_ => _.AddConsole());

HttpClient _apiClient = new HttpClient(result.RefreshTokenHandler)
{
    BaseAddress = new Uri(ApiRppt)
};

var repository = new ClientHttpRepository(ApiRppt, _apiClient, factory.CreateLogger<ClientHttpRepository>());

Console.WriteLine("\n\nClaims:");
foreach (var claim in result.User.Claims)
{
    Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
}

Console.WriteLine($"\nidentity token: {result.IdentityToken}");
Console.WriteLine($"access token:   {result.AccessToken}");
Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");

var currentAccessToken = result.AccessToken;
var currentRefreshToken = result.RefreshToken;
_apiClient.SetBearerToken(currentAccessToken);

var menu = "  x...exit  c...call api   ";
if (currentRefreshToken != null) menu += "r...refresh token   ";

while (true)
{
    Console.WriteLine("\n\n");

    Console.Write(menu);
    var key = Console.ReadKey();

    if (key.Key == ConsoleKey.X) return;
    if (key.Key == ConsoleKey.C) await TestMethod.CallApi(repository, currentAccessToken);
    if (key.Key == ConsoleKey.R)
    {
        var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
        if (refreshResult.IsError)
        {
            Console.WriteLine($"Error: {refreshResult.Error}");
        }
        else
        {
            currentRefreshToken = refreshResult.RefreshToken;
            currentAccessToken = refreshResult.AccessToken;
            _apiClient.SetBearerToken(currentAccessToken);

            Console.WriteLine("\n\n");
            Console.WriteLine($"access token:   {refreshResult.AccessToken}");
            Console.WriteLine($"refresh token:  {refreshResult?.RefreshToken ?? "none"}");
        }
    }
}