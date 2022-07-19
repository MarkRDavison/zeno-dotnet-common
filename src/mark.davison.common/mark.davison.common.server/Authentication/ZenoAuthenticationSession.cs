namespace mark.davison.common.server.Authentication;

public class ZenoAuthenticationSession : IZenoAuthenticationSession
{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ZenoAuthenticationSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task CommitSessionAsync(CancellationToken cancellationToken)
    {
        return _httpContextAccessor.HttpContext!.Session.CommitAsync(cancellationToken);
    }

    public Task LoadSessionAsync(CancellationToken cancellationToken)
    {
        return _httpContextAccessor.HttpContext!.Session.LoadAsync(cancellationToken);
    }

    public void Clear()
    {
        _httpContextAccessor.HttpContext!.Session.Clear();
    }

    public string GetString(string key)
    {
        return _httpContextAccessor.HttpContext!.Session.GetString(key) ?? string.Empty;
    }

    public void Remove(string key)
    {
        _httpContextAccessor.HttpContext!.Session.Remove(key);
    }

    public void SetString(string key, string value)
    {
        _httpContextAccessor.HttpContext!.Session.SetString(key, value);
    }
}