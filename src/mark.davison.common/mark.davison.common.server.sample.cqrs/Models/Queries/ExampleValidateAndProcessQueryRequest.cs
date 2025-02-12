namespace mark.davison.common.server.sample.cqrs.Models.Queries;

[GetRequest(Path = "example-val-and-proc-query", AllowAnonymous = true)]
public class ExampleValidateAndProcessQueryRequest : IQuery<ExampleValidateAndProcessQueryRequest, ExampleValidateAndProcessQueryResponse>
{
}
