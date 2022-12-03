using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace mark.davison.common.server.test.Framework;

[TestClass]
public class IntegrationTestBase<TFactory, TSettings>
    where TFactory : ICommonWebApplicationFactory<TSettings>, IDisposable, new()
{
    protected TFactory _factory;

    public IntegrationTestBase()
    {
        _factory = new TFactory();
        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        _factory?.Dispose();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        await SeedDataInternal(Services.GetRequiredService<IServiceProvider>());
    }

    private async Task SeedDataInternal(IServiceProvider serviceProvider)
    {
        var healthState = serviceProvider.GetRequiredService<IApplicationHealthState>();
        await healthState.ReadySource.Task;
        await SeedData(serviceProvider);
    }
    protected virtual async Task SeedData(IServiceProvider serviceProvider)
    {
        await Task.CompletedTask;
    }

    protected async Task<HttpResponseMessage> CallAsync(HttpMethod httpMethod, string uri, object? data)
    {
        var message = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(uri, UriKind.Relative),
            Content = data == null ? null : new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
        };

        return await Client.SendAsync(message);
    }

    protected async Task<List<T>> GetMultipleAsync<T>(string uri, bool requireSuccess = false)
    {
        var response = await CallAsync(HttpMethod.Get, uri, null);
        if (requireSuccess)
        {
            response.EnsureSuccessStatusCode();
        }
        return await ReadAsAsync<List<T>>(response);
    }

    protected async Task<string> GetRawAsync(string uri, bool requireSuccess = false)
    {
        var response = await CallAsync(HttpMethod.Get, uri, null);
        if (requireSuccess)
        {
            response.EnsureSuccessStatusCode();
        }
        return await response.Content.ReadAsStringAsync();
    }

    protected async Task<T> GetAsync<T>(string uri, bool requireSuccess = false)
    {
        var response = await CallAsync(HttpMethod.Get, uri, null);
        if (requireSuccess)
        {
            response.EnsureSuccessStatusCode();
        }
        return await ReadAsAsync<T>(response);
    }

    protected async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
    {
        string res = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(res, SerializationHelpers.CreateStandardSerializationOptions())!;
    }

    protected HttpClient Client { get; }

    protected IServiceProvider Services => _factory.ServiceProvider;
}