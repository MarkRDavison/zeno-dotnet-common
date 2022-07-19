namespace mark.davison.common.server.abstractions.Repository;

public class HeaderParameters : Dictionary<string, string>
{
    public void CopyHeaders(HttpRequestMessage request)
    {
        foreach (var kv in this)
        {
            request.Headers.Add(kv.Key, kv.Value);
        }
    }
    public static HeaderParameters None => new HeaderParameters();
    public static HeaderParameters Auth(string token, User? user)
    {
        var headers = new HeaderParameters {
            { "Authorization", $"Bearer {token}" }
        };

        if (user != null)
        {
            headers.Add(ZenoAuthenticationConstants.HeaderNames.User, JsonSerializer.Serialize(user));
        }

        return headers;
    }
}

