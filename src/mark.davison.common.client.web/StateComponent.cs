namespace mark.davison.common.client.web;

public abstract class StateComponent : ComponentBase, IDisposable
{
    private bool _initialized;
    private bool _disposed;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeToStateChanges();
        _initialized = true;
    }

    protected virtual void SubscribeToStateChanges() { }
    protected virtual void UnsubscribeToStateChanges() { }

    protected async Task UpdateState()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_initialized is false)
        {
            Console.Error.WriteLine("You forgot to call base.OnInitialized on your class inheriting from StateComponent");
        }

        UnsubscribeToStateChanges();

        GC.SuppressFinalize(this);
    }
}
