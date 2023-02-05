using System.Linq.Expressions;

namespace mark.davison.common.tests.Repository;

[TestClass]
public class QueryParametersTests
{
    private class TestClass
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

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

    [TestMethod]
    public void Where_SerializesExpressionCorrectly()
    {
        Expression<Func<TestClass, bool>> where = _ => _.Id.StartsWith("a") && _.Name.Contains("b");

        var queryParams = new QueryParameters();
        queryParams.Where(where);

        Assert.IsTrue(queryParams.ContainsKey("where"));
        Assert.IsFalse(string.IsNullOrEmpty(queryParams["where"]));
    }

    [TestMethod]
    public void CreateBody_WhereNoBodyParameters_ReturnsEmpty()
    {
        var queryParams = new QueryParameters();

        Assert.IsTrue(string.IsNullOrEmpty(queryParams.CreateBody()));
    }

    [TestMethod]
    public void CreateBody_WhereBodyParameters_IncludesWhere_ReturnsNonEmpty()
    {
        var queryParams = new QueryParameters();

        queryParams.Where<TestClass>(_ => _.Id == "a");

        Assert.IsFalse(string.IsNullOrEmpty(queryParams.CreateBody()));
    }

    [TestMethod]
    public void CreateBody_WhereBodyParameters_IncludesProject_ReturnsNonEmpty()
    {
        var queryParams = new QueryParameters();

        queryParams["project"] = "to";

        Assert.IsFalse(string.IsNullOrEmpty(queryParams.CreateBody()));
    }
}
