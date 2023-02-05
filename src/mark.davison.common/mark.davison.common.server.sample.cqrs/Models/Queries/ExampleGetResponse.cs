namespace mark.davison.common.server.sample.cqrs.Models.Queries;

public class ExampleGetResponse
{
    public string ResponseValue { get; set; } = string.Empty;
    public DateOnly DateOnlyValue { get; set; }
}
