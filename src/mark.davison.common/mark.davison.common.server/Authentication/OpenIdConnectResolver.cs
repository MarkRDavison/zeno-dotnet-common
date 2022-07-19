namespace mark.davison.common.server.Authentication;

public class OpenIdConnectResolver : IHostedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ZenoAuthOptions _options;

    public OpenIdConnectResolver(
        IHttpClientFactory httpClientFactory,
        IOptions<ZenoAuthOptions> options
    )
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(ZenoAuthenticationConstants.AuthClientName);
        var response = await client.GetAsync(_options.OpenIdConnectWellKnownUri);
        _options.OpenIdConnectConfiguration = OpenIdConnectConfiguration.Create(await response.Content.ReadAsStringAsync());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}