using mark.davison.common.Repository;

namespace mark.davison.common.server.Repository;

public abstract class HttpRepository : IHttpRepository
{
    private readonly string _remoteEndpoint;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;


    protected HttpRepository(string remoteEndpoint, HttpClient httpClient)
    {
        _remoteEndpoint = remoteEndpoint.TrimEnd('/');
        _httpClient = httpClient;
        _options = SerializationHelpers.CreateStandardSerializationOptions();
    }
    protected HttpRepository(string remoteEndpoint, HttpClient httpClient, JsonSerializerOptions options)
    {
        _remoteEndpoint = remoteEndpoint.TrimEnd('/');
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<List<T>> GetEntitiesAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        return await GetEntitiesAsync<T>(typeof(T).Name.ToLower(), query, header, cancellationToken);
    }

    private Uri CreateUriFromRelative(string relativeUri)
    {
        return string.IsNullOrEmpty(_remoteEndpoint)
                ? new Uri(relativeUri, UriKind.Relative)
                : new Uri($"{_remoteEndpoint}{relativeUri}", UriKind.Absolute);
    }

    public async Task<List<T>> GetEntitiesAsync<T>(string path, QueryParameters query, HeaderParameters header, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(path))
        {
            path = "/" + path.TrimStart('/');
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            CreateUriFromRelative($"/api{path}{query.CreateQueryString()}"));
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<T>();
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<T[]>(content, _options);
        return result?.ToList() ?? new List<T>();
    }

    public async Task<T?> GetEntityAsync<T>(Guid id, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entityRouteName = typeof(T).Name.ToLowerInvariant();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            CreateUriFromRelative($"/api/{entityRouteName}/{id}"));
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _options);
    }

    // TODO: Make this a separate endpoint in the service, only one entity in network traffic required.
    public async Task<T?> GetEntityAsync<T>(QueryParameters query, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entities = await GetEntitiesAsync<T>(query, header, cancellationToken);
        return entities.FirstOrDefault();
    }

    [ExcludeFromCodeCoverage] // TODO: This is bad
    public async Task<List<T>> UpsertEntitiesAsync<T>(List<T> entities, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var persisted = new List<T>();
        foreach (var entity in entities)
        {
            var p = await UpsertEntityAsync<T>(entity, header, cancellationToken);
            if (p != null)
            {
                persisted.Add(p);
            }
        }
        return persisted;
    }

    public async Task<T?> UpsertEntityAsync<T>(T entity, HeaderParameters header, CancellationToken cancellationToken) where T : BaseEntity
    {
        var entityRouteName = typeof(T).Name.ToLowerInvariant();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = CreateUriFromRelative($"/api/{entityRouteName}"),
            Content = new StringContent(JsonSerializer.Serialize(entity, _options), Encoding.UTF8, WebUtilities.ContentType.Json)
        };
        header.CopyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _options);
    }
}