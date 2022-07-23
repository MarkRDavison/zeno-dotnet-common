namespace mark.davison.common.test;

[ExcludeFromCodeCoverage]
public class MockHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage>? SendAsyncFunc { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        if (SendAsyncFunc != null)
        {
            return SendAsyncFunc(request);
        }

        return new HttpResponseMessage(HttpStatusCode.OK);
    }
}
