using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace mark.davison.common.client.Store;

public static class StateStore
{
    private static readonly ConcurrentDictionary<Type, List<Action>> _stateChangeCallbacks;
    private static readonly ConcurrentDictionary<Type, object> _state;

    private static readonly ConcurrentDictionary<Type, List<Func<IServiceProvider, object, Task>>> _effectCallbacks;
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, List<Func<object, object, object>>>> _reducerCallbacks;

    static StateStore()
    {
        _stateChangeCallbacks = new();
        _state = new();
        _effectCallbacks = new();
        _reducerCallbacks = new();
    }

    public static async Task DispatchEffects(IServiceProvider services, object payload)
    {
        if (_effectCallbacks.TryGetValue(payload.GetType(), out var callbacks))
        {
            foreach (var c in callbacks)
            {
                await c(services, payload);
            }
        }
    }

    public static void ApplyReducers(IServiceProvider services, object payload)
    {
        if (_reducerCallbacks.TryGetValue(payload.GetType(), out var stateCallbacks))
        {
            foreach (var (stateType, callbacks) in stateCallbacks)
            {
                var stateImplementation = services.GetRequiredService(stateType) as StateImplementation;

                var state = (stateImplementation?.StateValue)
                    ?? throw new InvalidOperationException($"{stateType.Name} was not registered");

                foreach (var callback in callbacks)
                {
                    state = callback(state, payload);

                    if (state is null)
                    {
                        throw new InvalidOperationException($"State callback for {stateType.Name} did not return the correct type");
                    }
                }

                stateImplementation!.SetState(state);
            }
        }
    }

    public static void RegisterReducerCallback<TAction, TState>(Func<TState, TAction, TState> callback)
        where TAction : class, new()
        where TState : class, IClientState, new()
    {
        var callbacks = _reducerCallbacks.GetOrAdd(typeof(TAction), []);

        var stateCallbacks = callbacks.GetOrAdd(typeof(IState<TState>), []);

        stateCallbacks.Add((state, payload) => callback((TState)state, (TAction)payload));
    }

    public static void RegisterEffectCallback<TAction, TEffectClass>(Func<IServiceProvider, TAction, IDispatcher, Task> callback)
        where TEffectClass : class
        where TAction : class, new()
    {
        var callbacks = _effectCallbacks.GetOrAdd(typeof(TAction), []);

        callbacks.Add(async (_services, _action) =>
        {
            var dispatcher = _services.GetRequiredService<IDispatcher>();

            await callback(_services, (TAction)_action, dispatcher);
        });
    }

    public static void RegisterStateChangeCallback<TState>(Action callback) where TState : class, IClientState, new()
    {
        var callbacks = _stateChangeCallbacks.GetOrAdd(typeof(TState), []);
        callbacks.Add(callback);
    }

    public static TState GetState<TState>() where TState : class, IClientState, new()
    {
        return (TState)_state.GetOrAdd(typeof(TState), _ => new TState());
    }

    public static void SetState<TState>(TState state) where TState : class, IClientState, new()
    {
        _state[typeof(TState)] = state;

        if (_stateChangeCallbacks.TryGetValue(typeof(TState), out var callbacks))
        {
            // TODO: Threading -> wpf Dispatcher.UiThread
            foreach (var callback in callbacks)
            {
                callback();
            }
        }
    }
}
