namespace mark.davison.common.server.sample.api.Scenarios.ExampleGet;

[GetRequest(Path = "example-get")]
public class ExampleGetRequest : IQuery<ExampleGetRequest, ExampleGetResponse>
{
    public string RequestValue { get; set; } = string.Empty;
    public DateOnly DateOnlyValue { get; set; }
}
