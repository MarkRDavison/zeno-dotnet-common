namespace mark.davison.common.client.Store;

public class ActionSubscriber : IActionSubscriber
{
    private readonly Lock SyncRoot = new();
    private readonly Dictionary<object, List<ActionSubscription>> SubscriptionsForInstance = new();
    private readonly Dictionary<Type, List<ActionSubscription>> SubscriptionsForType = new();

    public IDisposable GetActionUnsubscriberAsIDisposable(object subscriber) =>
        new DisposableCallback(
            id: $"{nameof(ActionSubscriber)}.{nameof(GetActionUnsubscriberAsIDisposable)}",
            action: () => UnsubscribeFromAllActions(subscriber));

    public void Notify(object action)
    {
        ArgumentNullException.ThrowIfNull(action);

        SyncRoot.Enter();

        IEnumerable<Action<object>> callbacks =
            [
                .. SubscriptionsForType
                    .Where(x => x.Key.IsAssignableFrom(action.GetType()))
                    .SelectMany(x => x.Value)
                    .Select(x => x.Callback)
            ];

        foreach (Action<object> callback in callbacks)
        {
            callback(action);
        }

        SyncRoot.Exit();
    }

    public void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(callback);

        var subscription = new ActionSubscription(
            subscriber: subscriber,
            actionType: typeof(TAction),
            callback: action => callback((TAction)action));

        SyncRoot.Enter();

        if (!SubscriptionsForInstance.TryGetValue(subscriber, out var instanceSubscriptions))
        {
            instanceSubscriptions = [];
            SubscriptionsForInstance[subscriber] = instanceSubscriptions;
        }

        instanceSubscriptions.Add(subscription);

        if (!SubscriptionsForType.TryGetValue(typeof(TAction), out var typeSubscriptions))
        {
            typeSubscriptions = [];
            SubscriptionsForType[typeof(TAction)] = typeSubscriptions;
        }

        typeSubscriptions.Add(subscription);

        SyncRoot.Exit();
    }


    public void UnsubscribeFromAllActions(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        if (!SubscriptionsForInstance.TryGetValue(subscriber, out var instanceSubscriptions))
        {
            return;
        }

        SyncRoot.Enter();

        IEnumerable<object> subscribedInstances =
            instanceSubscriptions
                .Select(x => x.Subscriber)
                .Distinct();

        IEnumerable<Type> subscribedActionTypes =
            instanceSubscriptions
                .Select(x => x.ActionType)
                .Distinct();

        foreach (Type actionType in subscribedActionTypes)
        {
            if (!SubscriptionsForType.TryGetValue(actionType, out var actionTypeSubscriptions))
            {
                continue;
            }

            SubscriptionsForType[actionType] = actionTypeSubscriptions
                .Except(instanceSubscriptions)
                .ToList();
        }

        foreach (object subscription in subscribedInstances)
        {
            SubscriptionsForInstance.Remove(subscription);
        }

        SyncRoot.Exit();
    }
}