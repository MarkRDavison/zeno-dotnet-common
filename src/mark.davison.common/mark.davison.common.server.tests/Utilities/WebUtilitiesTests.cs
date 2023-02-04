using Microsoft.Extensions.Primitives;
using System.Text;

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

    [TestMethod]
    public async Task GetRequestBody_WhereRequestContentLengthIsZero_ReturnsEmptyString()
    {
        Mock<HttpRequest> request = new();

        request.Setup(_ => _.ContentLength).Returns(0);

        var body = await WebUtilities.GetRequestBody(request.Object);

        Assert.AreEqual(string.Empty, body);
    }

    [TestMethod]
    public async Task GetRequestBody_WhereRequestContentLengthThrows_ReturnsEmptyString()
    {
        Mock<HttpRequest> request = new();

        request.Setup(_ => _.ContentLength).Throws<InvalidOperationException>();

        var body = await WebUtilities.GetRequestBody(request.Object);

        Assert.AreEqual(string.Empty, body);
    }

    [TestMethod]
    public async Task GetRequestBody_ReadsFromBodyStream()
    {
        string bodyContent = "0123456789";
        Mock<HttpRequest> request = new();

        request.Setup(_ => _.ContentLength).Returns(bodyContent.Length);
        request.Setup(_ => _.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(bodyContent)));

        var body = await WebUtilities.GetRequestBody(request.Object);

        Assert.AreEqual(bodyContent, body);
    }

    [TestMethod]
    public async Task GetRequestFromBody_WhereBodyIsEmpty_ReturnsNewRequest()
    {
        Mock<HttpRequest> request = new();

        request.Setup(_ => _.ContentLength).Returns(0);

        var value = await WebUtilities.GetRequestFromBody<ExampleCommandRequest, ExampleCommandResponse>(request.Object);

        Assert.AreEqual(string.Empty, value.Name);
        Assert.AreEqual(0, value.Value);
    }

    [TestMethod]
    public async Task GetRequestFromBody_WhereDeserializeReturnsNull_ReturnsNewRequest()
    {
        Mock<HttpRequest> request = new();

        request.Setup(_ => _.ContentLength).Returns(4);
        request.Setup(_ => _.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("null")));

        var value = await WebUtilities.GetRequestFromBody<ExampleCommandRequest, ExampleCommandResponse>(request.Object);

        Assert.AreEqual(string.Empty, value.Name);
        Assert.AreEqual(0, value.Value);
    }

    [TestMethod]
    public async Task GetRequestFromBody_WhereDeserializeReturnsValid_ReturnsDeserializedRequest()
    {
        Mock<HttpRequest> request = new();
        ExampleCommandRequest command = new()
        {
            Name = "the name",
            Value = 123
        };

        string body = JsonSerializer.Serialize(command, SerializationHelpers.CreateStandardSerializationOptions()); ;

        request.Setup(_ => _.ContentLength).Returns(body.Length);
        request.Setup(_ => _.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(body)));

        var value = await WebUtilities.GetRequestFromBody<ExampleCommandRequest, ExampleCommandResponse>(request.Object);

        Assert.AreEqual(command.Name, value.Name);
        Assert.AreEqual(command.Value, value.Value);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForGuidProperty_WhereValid_Works()
    {
        Guid property = Guid.NewGuid();
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues> 
        {
            { nameof(ExampleQueryRequest.Guid), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.Guid);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForGuidProperty_WhereInvalid_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Guid), "INVALID_GUID_FORMAT" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForStringProperty_WhereValid_Works()
    {
        string property = "some-string-value";
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.String), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.String);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForLongProperty_WhereValid_Works()
    {
        long property = 12465;
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Long), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.Long);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForLongProperty_WhereInvalid_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Long), "INVALID_LONG_FORMAT" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForIntProperty_WhereValid_Works()
    {
        int property = 12465;
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Int), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.Int);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForIntProperty_WhereInvalid_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Int), "INVALID_INT_FORMAT" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForBoolProperty_WhereValid_Works()
    {
        bool property = true;
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Bool), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.Bool);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForBoolProperty_WhereInvalid_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Bool), "INVALID_BOOL_FORMAT" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForDateOnlyProperty_WhereValid_Works()
    {
        DateOnly property = DateOnly.FromDateTime(DateTime.Today);
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.DateOnly), property.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(property, value.DateOnly);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForDateOnlyProperty_WhereInvalid_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.DateOnly), "INVALID_DATE_ONLY_FORMAT" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetRequestFromQuery_ForUnhandledProperty_Throws()
    {
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.Invalid), "INVALID_PROPERTY_TYPE" }
        }));

        WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);
    }

    [TestMethod]
    public void GetRequestFromQuery_ForValidQuery_ReturnsValidRequest()
    {
        var queryRequest = new ExampleQueryRequest
        {
            String = "some-string-value",
            Guid = Guid.NewGuid(),
            Long = 123,
            Int = 512,
            DateOnly = DateOnly.FromDateTime(DateTime.Today),
        };
        Mock<HttpRequest> request = new();
        request.Setup(_ => _.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
        {
            { nameof(ExampleQueryRequest.String), queryRequest.String.ToString() },
            { nameof(ExampleQueryRequest.Guid), queryRequest.Guid.ToString() },
            { nameof(ExampleQueryRequest.Long), queryRequest.Long.ToString() },
            { nameof(ExampleQueryRequest.Int), queryRequest.Int.ToString() },
            { nameof(ExampleQueryRequest.DateOnly), queryRequest.DateOnly.ToString() }
        }));

        var value = WebUtilities.GetRequestFromQuery<ExampleQueryRequest, ExampleQueryResponse>(request.Object);

        Assert.AreEqual(queryRequest.String, value.String);
        Assert.AreEqual(queryRequest.Guid, value.Guid);
        Assert.AreEqual(queryRequest.Long, value.Long);
        Assert.AreEqual(queryRequest.Int, value.Int);
        Assert.AreEqual(queryRequest.DateOnly, value.DateOnly);
    }
}
