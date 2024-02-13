namespace mark.davison.common.tests.Utility;

public class TestHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (Callback != null)
        {
            return await Callback(request);
        }

        throw new NotImplementedException();
    }

    public Func<HttpRequestMessage, Task<HttpResponseMessage>>? Callback { get; set; }
}