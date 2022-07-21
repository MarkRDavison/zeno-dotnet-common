using mark.davison.common.client.Authentication;

namespace mark.davison.common.client.tests.Authentication;

[TestClass]
public class CookieHandlerTests
{
    [TestMethod]
    public async Task SendAsync_SetsCredentialsToInclude()
    {
        await Task.CompletedTask;

        var cookieHandler = new CookieHandler
        {
            InnerHandler = new TestHandler()
        };

        var request = new HttpRequestMessage();
        var invoker = new HttpMessageInvoker(cookieHandler);
        await invoker.SendAsync(request, new CancellationToken());

        var val = request.Options.FirstOrDefault().Value as Dictionary<string, object>;
        Assert.IsNotNull(val);
        Assert.IsTrue(val.Any(kv => kv.Key == "credentials" && kv.Value as string == "include"));
    }
}
