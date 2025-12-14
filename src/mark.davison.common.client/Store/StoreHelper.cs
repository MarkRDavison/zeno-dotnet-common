namespace mark.davison.common.client.Store;

public class StoreHelper : IStoreHelper
{
    private readonly IDispatcher _dispatcher;
    private readonly IActionSubscriber _actionSubscriber;

    public StoreHelper(
        IDispatcher dispatcher,
        IActionSubscriber actionSubscriber)
    {
        _dispatcher = dispatcher;
        _actionSubscriber = actionSubscriber;
    }

    public TimeSpan DefaultRefetchTimeSpan => TimeSpan.FromMinutes(1);
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(60);

    public IDisposable ConditionalForce(bool force)
    {
        throw new NotImplementedException();
    }

    public void Dispatch<TAction>(TAction action) where TAction : BaseAction, new()
    {
        _dispatcher.Dispatch(action);
    }

    public async Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new()
    {
        return await DispatchAndWaitForResponse<TAction, TActionResponse>(action, DefaultTimeout);
    }

    public async Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action, TimeSpan timeout)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new()
    {
        TaskCompletionSource tcs = new();
        TActionResponse? result = null;

        _actionSubscriber.SubscribeToAction(
                this,
                (TActionResponse actionResponse) =>
                {
                    bool match = false;

                    if (actionResponse.ActionId == action.ActionId)
                    {
                        match = true;
                    }

                    if (match)
                    {
                        result = actionResponse;
                        tcs.SetResult();
                    }
                });

        using (_actionSubscriber.GetActionUnsubscriberAsIDisposable(this))
        {
            _dispatcher.Dispatch(action);

            await Task.WhenAny(tcs.Task, Task.Delay(timeout));
        }

        if (result == null)
        {
            return new TActionResponse
            {
                ActionId = action.ActionId,
                Errors = ["TIMED OUT"]
            };
        }

        return result;
    }

    public void DispatchWithThrottle<TAction>(DateTime lastDispatched, TAction action) where TAction : BaseAction, new()
    {
        throw new NotImplementedException();
    }

    public Task DispatchWithThrottleAndWaitForResponse<TAction, TActionResponse>(DateTime lastDispatched, TAction action)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new()
    {
        throw new NotImplementedException();
    }

    public IDisposable Force()
    {
        throw new NotImplementedException();
    }
}
