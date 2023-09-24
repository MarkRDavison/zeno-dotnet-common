namespace mark.davison.common.source.generators.CQRS;

public enum CQRSActivityType
{
    Command,
    Query,
    Action,
    ResponseAction
}

[ExcludeFromCodeCoverage]
public class CQRSActivity
{
    public CQRSActivityType Type { get; set; }
    public string Request { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Handler { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public string Validator { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool Anonymous { get; set; }
}
