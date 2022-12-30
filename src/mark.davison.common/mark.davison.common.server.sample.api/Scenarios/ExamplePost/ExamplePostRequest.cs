namespace mark.davison.common.server.sample.api.Scenarios.ExamplePost;

public enum TestEnum
{
    ValueOne,
    ValueTwo,
    ValueThree,
    ValueFour
}

[PostRequest(Path = "example-post-request")]
public class ExamplePostRequest : ICommand<ExamplePostRequest, ExamplePostResponse>
{
    public TestEnum TestEnumValue { get; set; }
}
