namespace mark.davison.common.client.Store;

public class StoreHelper : IStoreHelper
{
    internal bool _force;

    private readonly StoreHelperOptions _options;
    private readonly IDateService _dateService;
    private readonly IDispatcher _dispatcher;
    private readonly IActionSubscriber _actionSubscriber;

    private class StoreHelperDisposable : IDisposable
    {
        private bool _disposedValue;
        private readonly StoreHelper _stateHelper;

        public StoreHelperDisposable(StoreHelper stateHelper, bool forced)
        {
            _stateHelper = stateHelper;
            _stateHelper._force = forced;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _stateHelper._force = false;
                    _disposedValue = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

    public StoreHelper(
        StoreHelperOptions options,
        IDateService dateService,
        IDispatcher dispatcher,
        IActionSubscriber actionSubscriber)
    {
        _options = options;
        _dateService = dateService;
        _dispatcher = dispatcher;
        _actionSubscriber = actionSubscriber;
    }

    public IDisposable Force() => ConditionalForce(true);
    public IDisposable ConditionalForce(bool force) => new StoreHelperDisposable(this, force);

    private bool RequiresRefetch(DateTime stateLastModified, TimeSpan interval)
    {
        return _force || _dateService.Now - stateLastModified > interval;
    }

    public TimeSpan DefaultRefetchTimeSpan => TimeSpan.FromMinutes(1);
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(60);


    public void Dispatch<TAction>(TAction action)
        where TAction : BaseAction
    {
        _dispatcher.Dispatch(action);
    }

    public void DispatchWithThrottle<TAction>(DateTime lastDispatched, TAction action)
        where TAction : BaseAction
    {
        if (!RequiresRefetch(lastDispatched, DefaultRefetchTimeSpan))
        {
            return;
        }

        _dispatcher.Dispatch(action);
    }

    public async Task DispatchWithThrottleAndWaitForResponse<TAction, TActionResponse>(DateTime lastDispatched, TAction action)
        where TAction : BaseAction
        where TActionResponse : BaseActionResponse, new()
    {
        if (!RequiresRefetch(lastDispatched, DefaultRefetchTimeSpan))
        {
            return;
        }

        await DispatchAndWaitForResponse<TAction, TActionResponse>(action);
    }


    public async Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action)
        where TAction : BaseAction
        where TActionResponse : BaseActionResponse, new()
    {
        return await DispatchAndWaitForResponse<TAction, TActionResponse>(action, DefaultTimeout);
    }

    public async Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action, TimeSpan timeout)
        where TAction : BaseAction
        where TActionResponse : BaseActionResponse, new()
    {
        TaskCompletionSource tcs = new();
        TActionResponse? result = null;

        _actionSubscriber.SubscribeToAction(
                this,
                (TActionResponse actionResponse) =>
                {
                    bool match = false;
                    if (_options.ResponseCompareFunction != null)
                    {
                        match = _options.ResponseCompareFunction.Invoke(action, actionResponse);
                    }
                    else
                    {
                        if (actionResponse.ActionId == action.ActionId)
                        {
                            match = true;
                        }

                        if (_options.VerboseActionIdComparison)
                        {
                            Console.WriteLine(
                                "Comparing action of type {0} with id {1} to response of type {2} with id {3}, match: {4}",
                                typeof(TAction).Name,
                                action.ActionId,
                                typeof(TActionResponse).Name,
                                actionResponse.ActionId,
                                match);
                        }
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
}
