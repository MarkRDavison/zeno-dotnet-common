namespace mark.davison.common.server.Middleware;

[ExcludeFromCodeCoverage]
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IZenoAuthenticationSession _zenoAuthenticationSession;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        IZenoAuthenticationSession zenoAuthenticationSession)
    {
        _next = next;
        _zenoAuthenticationSession = zenoAuthenticationSession;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        if (!context.Request.Path.ToString().Contains("health"))
        {
            Console.WriteLine("========== REQ START ==========");
            Console.WriteLine("REQUEST: {0} {1}", context.Request.Method, context.Request.Path);
        }

        await _next.Invoke(context);

        if (!context.Request.Path.ToString().Contains("health"))
        {
            Console.WriteLine("RESPONSE: {0}", context.Response.StatusCode);
            if (context.Response.StatusCode == 401)
            {
                await _zenoAuthenticationSession.LoadSessionAsync(CancellationToken.None);
                var access = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.AccessToken);
                var refresh = _zenoAuthenticationSession.GetString(ZenoAuthenticationConstants.SessionNames.RefreshToken);

                if (string.IsNullOrEmpty(access))
                {
                    Console.WriteLine("Access token is empty");
                }
                if (string.IsNullOrEmpty(refresh))
                {
                    Console.WriteLine("Refresh token is empty");
                }
            }
            Console.WriteLine("========== REQ END ==========");
        }
    }

}