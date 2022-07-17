namespace mark.davison.common.client.State;

public class ComponentSubscriptions : IComponentSubscriptions
{
    private IDictionary<Type, List<IComponentWithState>> _components;

    public ComponentSubscriptions()
    {
        _components = new Dictionary<Type, List<IComponentWithState>>();
    }

    public void Add<TState>(IComponentWithState component) where TState : class, IState, new()
    {
        if (!_components.ContainsKey(typeof(TState)))
        {
            _components[typeof(TState)] = new List<IComponentWithState>();
        }

        _components[typeof(TState)].Add(component);
    }

    public void Remove(IComponentWithState component)
    {
        foreach (var (_, components) in _components)
        {
            components.Remove(component);
        }
    }

    public void ReRenderSubscribers(Type stateType)
    {
        if (_components.ContainsKey(stateType))
        {
            foreach (var component in _components[stateType])
            {
                component.ReRender();
            }
        }
    }

    public void ReRenderSubscribers<TState>() where TState : class, IState, new()
    {
        ReRenderSubscribers(typeof(TState));
    }

}
