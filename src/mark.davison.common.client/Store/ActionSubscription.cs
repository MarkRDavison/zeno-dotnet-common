namespace mark.davison.common.client.Store;

internal class ActionSubscription
{
    public readonly object Subscriber;
    public readonly Type ActionType;
    public readonly Action<object> Callback;

    public ActionSubscription(object subscriber, Type actionType, Action<object> callback)
    {
        Subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }
}
