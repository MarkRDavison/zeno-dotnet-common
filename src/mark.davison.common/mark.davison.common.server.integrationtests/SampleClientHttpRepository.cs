namespace mark.davison.common.server.integrationtests;

public class SampleClientHttpRepository : ClientHttpRepository
{
    public SampleClientHttpRepository(
        string remoteEndpoint,
        HttpClient httpClient,
        ILogger<SampleClientHttpRepository> logger
    ) : base(
        remoteEndpoint,
        httpClient,
        logger)
    {
    }
}
