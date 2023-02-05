namespace mark.davison.common.tests.CQRS;

[TestClass]
public class ResponseTests
{
    [TestMethod]
    public void BaseResponse_WhereWarnings_IsSuccess()
    {
        var response = new Response
        {
            Warnings = new List<string> { "Warn" }
        };

        Assert.IsTrue(response.Success);
    }

    [TestMethod]
    public void BaseResponse_WhereErrors_IsNotSuccess()
    {
        var response = new Response
        {
            Errors = new List<string> { "Error" }
        };

        Assert.IsFalse(response.Success);
    }

    [TestMethod]
    public void Response_WhereWarningsAndValue_IsSuccess()
    {
        var response = new Response<object>
        {
            Value = new(),
            Warnings = new List<string> { "Warn" }
        };

        Assert.IsTrue(response.Success);
        Assert.IsTrue(response.SuccessWithValue);
    }

    [TestMethod]
    public void Response_WhereWarningsAndNoValue_IsNotSuccess()
    {
        var response = new Response<object>
        {
            Value = null,
            Warnings = new List<string> { "Warn" }
        };

        Assert.IsTrue(response.Success);
        Assert.IsFalse(response.SuccessWithValue);
    }

    [TestMethod]
    public void Response_WhereErrorsAndValue_IsNotSuccess()
    {
        var response = new Response<object>
        {
            Value = new(),
            Errors = new List<string> { "Error" }
        };

        Assert.IsFalse(response.Success);
        Assert.IsFalse(response.SuccessWithValue);
    }

    [TestMethod]
    public void Response_WhereErrorsAndNoValue_IsNotSuccess()
    {
        var response = new Response<object>
        {
            Value = null,
            Errors = new List<string> { "Error" }
        };

        Assert.IsFalse(response.Success);
        Assert.IsFalse(response.SuccessWithValue);
    }
}
