namespace mark.davison.common.server.sample.api;

public class SampleHttpRepository : HttpRepository
{
    public SampleHttpRepository(string remoteEndpoint, HttpClient httpClient, JsonSerializerOptions options) : base(remoteEndpoint, httpClient, options)
    {
    }
    public SampleHttpRepository(string remoteEndpoint, JsonSerializerOptions options) : base(remoteEndpoint, new HttpClient(), options)
    {
    }
}
