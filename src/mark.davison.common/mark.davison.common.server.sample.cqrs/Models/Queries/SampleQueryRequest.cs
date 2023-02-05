namespace mark.davison.common.server.sample.cqrs.Models.Queries;

[GetRequest(Path = "sample-query")]
public class SampleQueryRequest : IQuery<SampleQueryRequest, SampleQueryResponse>
{
}
