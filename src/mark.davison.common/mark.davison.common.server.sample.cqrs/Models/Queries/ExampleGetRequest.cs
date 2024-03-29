﻿namespace mark.davison.common.server.sample.cqrs.Models.Queries;


[GetRequest(Path = "example-get", AllowAnonymous = true)]
public class ExampleGetRequest : IQuery<ExampleGetRequest, ExampleGetResponse>
{
    public string RequestValue { get; set; } = string.Empty;
    public DateOnly DateOnlyValue { get; set; }
}
