namespace mark.davison.common.server.Utilities;

public static class WebUtilities
{
    public static class ContentType
    {
        public const string Json = "application/json";
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
    }

    public static Uri CreateQueryUri(string uri, IDictionary<string, string> queryParams)
    {
        var encodedQueryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value)));
        var url = new UriBuilder(uri);
        url.Query = string.Join("&", encodedQueryStringParams);

        return url.Uri;
    }

    public static RedirectResult RedirectPreserveMethod(string uri)
    {
        return new RedirectResult(uri, false, true);
    }

    public static string CreateBearerHeaderValue(string token) => "Bearer " + token;
}