namespace mark.davison.common.client.abstractions.Store;

public interface IActionSubscriber
{
    public void Notify(object action);
    void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback);
    IDisposable GetActionUnsubscriberAsIDisposable(object subscriber);
}
