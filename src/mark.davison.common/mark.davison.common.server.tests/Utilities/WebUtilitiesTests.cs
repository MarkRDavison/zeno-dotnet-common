namespace mark.davison.common.server.tests.Utilities;

[TestClass]
public class WebUtilitiesTests
{
    [TestMethod]
    public void CreateBearerHeaderValue_AppendsBearer()
    {
        var token = "XYZ";

        Assert.AreEqual($"Bearer {token}", WebUtilities.CreateBearerHeaderValue(token));
    }

    [TestMethod]
    public void RedirectPreserveMethod_PopulatesResultAsExpected()
    {
        var result = WebUtilities.RedirectPreserveMethod("http://localhost:8080");

        Assert.IsFalse(result.Permanent);
        Assert.IsTrue(result.PreserveMethod);
    }

    [TestMethod]
    public void CreateQueryUri_CreatesExpectedResult()
    {
        var baseUri = "http://localhost:8080/entity";
        var queryParams = new Dictionary<string, string> {
            { "a", "b" }
        };
        Assert.AreEqual($"{baseUri}?a=b", WebUtilities.CreateQueryUri(baseUri, queryParams).ToString());
        queryParams.Add("c", "d");
        Assert.AreEqual($"{baseUri}?a=b&c=d", WebUtilities.CreateQueryUri(baseUri, queryParams).ToString());
        queryParams.Add("e", "f");
        Assert.AreEqual($"{baseUri}?a=b&c=d&e=f", WebUtilities.CreateQueryUri(baseUri, queryParams).ToString());
    }
}
