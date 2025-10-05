namespace mark.davison.common.source.generators.CQRS;

public class CqrsSourceGeneratorActivity
{
    public CqrsSourceGeneratorActivity(
        bool isRequestDefinition,
        CQRSActivityType? type,
        string request,
        string response,
        string? handler,
        string? validator,
        string? processor,
        string? endpoint)
    {
        IsRequestDefinition = isRequestDefinition;
        Type = type;
        Request = request;
        Response = response;
        Handler = handler;
        Validator = validator;
        Processor = processor;
        Endpoint = endpoint;
    }

    public bool IsRequestDefinition { get; }
    public CQRSActivityType? Type { get; }
    public string Request { get; }
    public string Response { get; }
    public string? Handler { get; }
    public string? Validator { get; }
    public string? Processor { get; }
    public string? Endpoint { get; }

    public string Key => $"{Type}_{Request}_{Response}";
}
