namespace mark.davison.common.CQRS;

public class Response
{
    public bool Success => !Errors.Any();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class Response<T> : Response
{
    public T? Value { get; set; }
}