namespace mark.davison.common.client.abstractions.Store;

public interface IStoreHelper
{
    IDisposable Force();
    IDisposable ConditionalForce(bool force);
    TimeSpan DefaultRefetchTimeSpan { get; }
    TimeSpan DefaultTimeout { get; }

    void Dispatch<TAction>(TAction action)
        where TAction : BaseAction, new();

    void DispatchWithThrottle<TAction>(DateTime lastDispatched, TAction action)
        where TAction : BaseAction, new();

    Task DispatchWithThrottleAndWaitForResponse<TAction, TActionResponse>(DateTime lastDispatched, TAction action)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new();

    Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new();

    Task<TActionResponse> DispatchAndWaitForResponse<TAction, TActionResponse>(TAction action, TimeSpan timeout)
        where TAction : BaseAction, new()
        where TActionResponse : BaseActionResponse, new();
}
