namespace mark.davison.common.server.sample.cqrs.Models.Commands;

public enum TestEnum
{
    ValueOne,
    ValueTwo,
    ValueThree,
    ValueFour
}

[PostRequest(Path = "example-post-request", AllowAnonymous = true)]
public class ExamplePostRequest : ICommand<ExamplePostRequest, ExamplePostResponse>
{
    public TestEnum TestEnumValue { get; set; }
}
