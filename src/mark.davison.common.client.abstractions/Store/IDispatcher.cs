namespace mark.davison.common.client.abstractions.Store;

public interface IDispatcher
{
    void Dispatch<TAction>(TAction action) where TAction : class, new();
}
