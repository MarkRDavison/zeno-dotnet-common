namespace mark.davison.common.server.Repository;

public abstract class HttpRepository : IHttpRepository
{
    private readonly string _remoteEndpoint;
    private HttpClient _httpClient;


    protected HttpRepository(string remoteEndpoint)
    {
        _remoteEndpoint = remoteEndpoint;
        _httpClient = new HttpClient();
    }

    public async Task<List<T>> GetEntitiesAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entityRouteName = typeof(T).Name.ToLowerInvariant();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_remoteEndpoint}/api/{entityRouteName}{query.CreateQueryString()}");
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<T[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.ToList() ?? new List<T>();
    }
    public async Task<List<T>> GetEntitiesAsync<T>(string path, QueryParameters query, HeaderParameters header, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_remoteEndpoint}/api/{path}{query.CreateQueryString()}");
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<T[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.ToList() ?? new List<T>();
    }

    public async Task<T?> GetEntityAsync<T>(Guid id, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entityRouteName = typeof(T).Name.ToLowerInvariant();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_remoteEndpoint}/api/{entityRouteName}/{id}");
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<T?> GetEntityAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entities = await GetEntitiesAsync<T>(query, header, cancellationToken);
        return entities.FirstOrDefault();
    }

    public async Task<List<T>> UpsertEntitiesAsync<T>(List<T> entities, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Create post multiple endpoint");
    }

    public async Task<T?> UpsertEntityAsync<T>(T entity, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entityRouteName = typeof(T).Name.ToLowerInvariant();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{_remoteEndpoint}/api/{entityRouteName}"),
            Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, WebUtilities.ContentType.Json)
        };
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}