var authConfig = new AuthenticationConfig();
authConfig.SetBffBase(WebConstants.LocalBffRoot);
authConfig.HttpClientName = WebConstants.ApiClientName;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    })
    .AddExampleWeb(authConfig);

await builder.Build().RunAsync();