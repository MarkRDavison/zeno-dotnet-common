namespace mark.davison.common.Utility;

public sealed class DisposableCallback : IDisposable
{
    private readonly string Id;
    private readonly Action Action;
    private bool Disposed;
    private readonly bool WasCreated;
    private readonly string CallerFilePath;
    private readonly int CallerLineNumber;

    public DisposableCallback(string id, Action action,
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0
        )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }
        ArgumentNullException.ThrowIfNull(action);

        Id = id;
        Action = action;
        WasCreated = true;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public void Dispose()
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(
                nameof(DisposableCallback),
                $"Attempt to call {nameof(Dispose)} twice on {nameof(DisposableCallback)} with Id {GetIdInfo()}.");
        }

        Disposed = true;
        GC.SuppressFinalize(this);
        Action();
    }

    private string GetIdInfo() => $"\"{Id}\" (created in \"{CallerFilePath}\" on line {CallerLineNumber})";

    /// <summary>
    /// Throws an exception if this object is collected without being disposed
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the object is collected without being disposed</exception>
    ~DisposableCallback()
    {
        if (!Disposed && WasCreated)
        {
            throw new InvalidOperationException($"{nameof(DisposableCallback)} with Id \"{GetIdInfo()}\" was not disposed");
        }
    }
}