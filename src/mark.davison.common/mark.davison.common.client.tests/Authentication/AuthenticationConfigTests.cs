namespace mark.davison.common.client.tests.Authentication;

[TestClass]
public class AuthenticationConfigTests
{
    [TestMethod]
    public void SetBffBase_SetsEndpointsCorrectly()
    {
        var bffBase = "https://localhost:8080/";
        var config = new AuthenticationConfig();
        config.SetBffBase(bffBase);

        Assert.AreEqual($"{bffBase}auth/login", config.LoginEndpoint);
        Assert.AreEqual($"{bffBase}auth/logout", config.LogoutEndpoint);
        Assert.AreEqual($"{bffBase}auth/user", config.UserEndpoint);
        Assert.AreEqual(bffBase.TrimEnd('/'), config.BffBase);
    }
}
