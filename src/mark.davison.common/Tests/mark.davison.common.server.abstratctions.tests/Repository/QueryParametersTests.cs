namespace mark.davison.common.server.abstratctions.tests.Repository;

[TestClass]
public class QueryParametersTests
{

    [DataTestMethod]
    [DataRow("a|b", "a", "b")]
    [DataRow("a|b|c|d|e", "a", "b", "c", "d", "e")]
    [DataRow("a", "a")]
    public void Include_SetsCorrectValue(string expected, params string[] paths)
    {
        var queryParams = new QueryParameters();
        foreach (var path in paths)
        {
            queryParams.Include(path);
        }

        Assert.AreEqual(expected, queryParams["include"]);
    }

    [TestMethod]
    public void CreateQueryString_HandlesEmpty()
    {
        var queryParams = new QueryParameters();

        Assert.AreEqual(string.Empty, queryParams.CreateQueryString());
    }

    [TestMethod]
    public void CreateQueryString_HandlesSingleValue()
    {
        var name = "name";
        var value = "value";

        var queryParams = new QueryParameters();
        queryParams.Add(name, value);

        Assert.AreEqual($"?{name}={value}", queryParams.CreateQueryString());
    }

    [TestMethod]
    public void CreateQueryString_HandlesMultipleValues()
    {
        var name1 = "name1";
        var value1 = "value1";
        var name2 = "name2";
        var value2 = "value2";

        var queryParams = new QueryParameters();
        queryParams.Add(name1, value1);
        queryParams.Add(name2, value2);

        Assert.AreEqual($"?{name1}={value1}&{name2}={value2}", queryParams.CreateQueryString());
    }
}
