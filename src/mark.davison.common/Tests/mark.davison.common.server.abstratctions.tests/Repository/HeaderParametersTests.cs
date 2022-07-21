namespace mark.davison.common.server.abstratctions.tests.Repository;

[TestClass]
public class HeaderParametersTests
{

    [TestMethod]
    public void None_CreatesEmptyParameterCollection()
    {
        var headers = HeaderParameters.None;

        Assert.IsFalse(headers.Any());
    }

    [TestMethod]
    public void Auth_CreatesAuthorizationHeader()
    {
        var token = "AUTH_TOKEN";
        var headers = HeaderParameters.Auth(token, null);

        Assert.IsTrue(headers[HeaderParameters.AuthHeaderName].EndsWith(token));
    }

    [TestMethod]
    public void Auth_WhereUserIsNull_DoesNotCreateUserHeader()
    {
        var headers = HeaderParameters.Auth("AUTH_TOKEN", null);

        Assert.IsFalse(headers.ContainsKey(ZenoAuthenticationConstants.HeaderNames.User));
    }

    [TestMethod]
    public void Auth_WhereUserIsNotNull_CreateUserHeader()
    {
        var headers = HeaderParameters.Auth("AUTH_TOKEN", new User());

        Assert.IsTrue(headers.ContainsKey(ZenoAuthenticationConstants.HeaderNames.User));
    }

    [TestMethod]
    public void CopyHeaders_CopiesAllHeaders_ToRequestMessage()
    {
        var headers = HeaderParameters.Auth("AUTH_TOKEN", new User());
        var headerNames = new List<string> {
            "a", "b", "c"
        };
        foreach (var header in headerNames)
        {
            headers[header] = "RANDOM VALUE";
        }

        var message = new HttpRequestMessage();
        headers.CopyHeaders(message);

        Assert.IsTrue(headers.Keys.All(message.Headers.Contains));
    }

}
