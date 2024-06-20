using mark.davison.common.client.abstractions.Repository;

namespace mark.davison.common.server.integrationtests.Tests.CQRS;

[TestClass]
public class CQRSTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{

    [TestMethod]
    public async Task ExampleGetQuery_WorksAsExpected()
    {
        var repository = Services.GetRequiredService<IClientHttpRepository>();
        var request = new ExampleGetRequest
        {
            RequestValue = "sihjbdsfuihjdfsiuhjdfs",
            DateOnlyValue = DateOnly.FromDateTime(DateTime.Now)
        };

        var response = await repository.Get<ExampleGetResponse, ExampleGetRequest>(request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreEqual(request.RequestValue, response.ResponseValue);
    }


    [TestMethod]
    public async Task ExamplePostQuery_WorksAsExpected()
    {
        var repository = Services.GetRequiredService<IClientHttpRepository>();
        var request = new ExamplePostRequest
        {
            TestEnumValue = TestEnum.ValueThree
        };

        var response = await repository.Post<ExamplePostResponse, ExamplePostRequest>(request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreEqual(request.TestEnumValue, response.TestEnumValue);
    }
}
