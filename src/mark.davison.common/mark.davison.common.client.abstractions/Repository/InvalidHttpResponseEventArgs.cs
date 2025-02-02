namespace mark.davison.common.client.abstractions.Repository;

public sealed class InvalidHttpResponseEventArgs : EventArgs
{
    public required HttpStatusCode Status { get; init; }
    public required HttpRequestMessage Request { get; init; }
    public bool Retry { get; set; }
    public Task RetryWaitTask { get; set; } = Task.CompletedTask;
}
