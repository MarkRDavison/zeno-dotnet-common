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
            RequestValue = "sihjbdsfuihjdfsiuhjdfs"
        };

        var response = await repository.Get<ExampleGetResponse, ExampleGetRequest>(request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreEqual(request.RequestValue, response.ResponseValue);
    }
}
