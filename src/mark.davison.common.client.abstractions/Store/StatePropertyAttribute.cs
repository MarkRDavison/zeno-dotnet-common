namespace mark.davison.common.client.abstractions.Store;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class StatePropertyAttribute<TState> : Attribute
    where TState : class, IClientState, new()
{
}
