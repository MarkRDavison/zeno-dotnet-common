namespace mark.davison.common.Utility;

public sealed class AsyncDisposableCallback : IAsyncDisposable
{
    private readonly string Id;
    private readonly Func<Task> AsyncAction;
    private bool Disposed;
    private readonly bool WasCreated;
    private readonly string CallerFilePath;
    private readonly int CallerLineNumber;

    public AsyncDisposableCallback(string id, Func<Task> asyncAction,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        ArgumentNullException.ThrowIfNull(asyncAction);

        Id = id;
        AsyncAction = asyncAction;
        WasCreated = true;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public async ValueTask DisposeAsync()
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(
                nameof(AsyncDisposableCallback),
                $"Attempt to call {nameof(DisposeAsync)} twice on {nameof(AsyncDisposableCallback)} with Id {GetIdInfo()}.");
        }

        Disposed = true;
        GC.SuppressFinalize(this);
        await AsyncAction().ConfigureAwait(false);
    }

    private string GetIdInfo() => $"\"{Id}\" (created in \"{CallerFilePath}\" on line {CallerLineNumber})";

    ~AsyncDisposableCallback()
    {
        if (!Disposed && WasCreated)
        {
            throw new InvalidOperationException($"{nameof(AsyncDisposableCallback)} with Id {GetIdInfo()} was not disposed");
        }
    }
}