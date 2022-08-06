namespace mark.davison.common.client.Repository;

public abstract class ClientHttpRepository : IClientHttpRepository
{
    private readonly string _remoteEndpoint;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ClientHttpRepository(string remoteEndpoint, HttpClient httpClient)
    {
        _remoteEndpoint = remoteEndpoint.TrimEnd('/');
        _httpClient = httpClient;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }
    public ClientHttpRepository(string remoteEndpoint, HttpClient httpClient, JsonSerializerOptions options)
    {
        _remoteEndpoint = remoteEndpoint.TrimEnd('/');
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<TResponse> Get<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IQuery<TRequest, TResponse>
        where TResponse : class, new()
    {
        // TODO: Source Generator-ify this
        var attribute = typeof(TRequest).CustomAttributes.FirstOrDefault(_ => _.AttributeType == typeof(GetRequestAttribute));
        if (attribute == null) { throw new InvalidOperationException("Cannot perform Get against request without GetRequestAttribute"); }

        var path = attribute.NamedArguments.First(_ => _.MemberName == nameof(GetRequestAttribute.Path));
        var pathValue = path.TypedValue.Value as string;

        var requestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_remoteEndpoint}/api/{pathValue!.TrimStart('/')}");
        using var response = await _httpClient.SendAsync(requestMessage);
        var body = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<TResponse>(body, _options);
        return obj ?? new TResponse();
    }

    public Task<TResponse> Get<TResponse, TRequest>(CancellationToken cancellationToken)
        where TRequest : class, IQuery<TRequest, TResponse>, new()
        where TResponse : class, new()
    {
        return Get<TResponse, TRequest>(new TRequest(), cancellationToken);
    }

    public async Task<TResponse> Post<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, ICommand<TRequest, TResponse>
        where TResponse : class, new()
    {
        // TODO: Source Generator-ify this
        var attribute = typeof(TRequest).CustomAttributes.FirstOrDefault(_ => _.AttributeType == typeof(PostRequestAttribute));
        if (attribute == null) { throw new InvalidOperationException("Cannot perform Post against request without PostRequestAttribute"); }

        var path = attribute.NamedArguments.First(_ => _.MemberName == nameof(PostRequestAttribute.Path));
        var pathValue = path.TypedValue.Value as string;

        var json = JsonSerializer.Serialize(request, _options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_remoteEndpoint}/api/{pathValue!.TrimStart('/')}")
        {
            Content = content
        };
        using var response = await _httpClient.SendAsync(requestMessage);
        var body = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<TResponse>(body, _options);
        return obj ?? new TResponse();
    }

    public Task<TResponse> Post<TResponse, TRequest>(CancellationToken cancellationToken)
        where TRequest : class, ICommand<TRequest, TResponse>, new()
        where TResponse : class, new()
    {
        return Post<TResponse, TRequest>(new TRequest(), cancellationToken);
    }
}
