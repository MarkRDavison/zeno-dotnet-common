namespace mark.davison.common.client.Repository;

public class ClientHttpRepository : IClientHttpRepository
{
    private readonly string _remoteEndpoint;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;
    private readonly ILogger _logger;

    public ClientHttpRepository(string remoteEndpoint, HttpClient httpClient, ILogger logger) : this(remoteEndpoint, httpClient, logger, SerializationHelpers.CreateStandardSerializationOptions())
    {
    }

    public ClientHttpRepository(string remoteEndpoint, HttpClient httpClient, ILogger logger, JsonSerializerOptions options)
    {
        _remoteEndpoint = remoteEndpoint.TrimEnd('/');
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<TResponse> Get<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IQuery<TRequest, TResponse>
        where TResponse : Response, new()
    {
        // TODO: Source Generator-ify this
        var attribute = typeof(TRequest).CustomAttributes.FirstOrDefault(_ => _.AttributeType == typeof(GetRequestAttribute));
        if (attribute == null) { throw new InvalidOperationException("Cannot perform Get against request without GetRequestAttribute"); }

        var path = attribute.NamedArguments.First(_ => _.MemberName == nameof(GetRequestAttribute.Path));
        var pathValue = path.TypedValue.Value as string;

        var queryParameters = CreateQueryParameters<TRequest, TResponse>(request);

        var requestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_remoteEndpoint}/api/{pathValue!.TrimStart('/')}{queryParameters.CreateQueryString()}");
        using var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            OnInvalidResponse?.Invoke(this, response.StatusCode);
        }

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var obj = JsonSerializer.Deserialize<TResponse>(body, _options);
            return obj ?? new TResponse();
        }
        catch (Exception e)
        {
            return new TResponse()
            {
                Errors = [e.Message]
            };
        }
    }

    public Task<TResponse> Get<TResponse, TRequest>(CancellationToken cancellationToken)
        where TRequest : class, IQuery<TRequest, TResponse>, new()
        where TResponse : Response, new()
    {
        return Get<TResponse, TRequest>(new TRequest(), cancellationToken);
    }

    public async Task<TResponse> Post<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, ICommand<TRequest, TResponse>
        where TResponse : Response, new()
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

        if (!response.IsSuccessStatusCode)
        {
            OnInvalidResponse?.Invoke(this, response.StatusCode);
        }

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var obj = JsonSerializer.Deserialize<TResponse>(body, _options);
            return obj ?? new TResponse();
        }
        catch (Exception e)
        {
            return new TResponse()
            {
                Errors = [e.Message]
            };
        }
    }

    public Task<TResponse> Post<TResponse, TRequest>(CancellationToken cancellationToken)
        where TRequest : class, ICommand<TRequest, TResponse>, new()
        where TResponse : Response, new()
    {
        return Post<TResponse, TRequest>(new TRequest(), cancellationToken);
    }

    public QueryParameters CreateQueryParameters<TQuery, TResponse>(TQuery query)
        where TQuery : class, IQuery<TQuery, TResponse>
        where TResponse : class, new()
    {
        var queryParameters = new QueryParameters();

        foreach (var property in typeof(TQuery).GetProperties())
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(query)?.ToString();

            if (!string.IsNullOrEmpty(propertyValue))
            {
                queryParameters.Add(propertyName, propertyValue);
            }
        }

        return queryParameters;
    }

    public event EventHandler<HttpStatusCode> OnInvalidResponse = default!;
}
