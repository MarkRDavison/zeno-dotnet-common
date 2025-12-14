namespace mark.davison.example.web.features.Store.StartupUseCase;

public sealed class StartupState : IClientState
{
    public StartupState() : this(false, new([], []))
    {

    }
    public StartupState(bool loaded, StartupQueryDto data)
    {
        Loaded = loaded;
        Data = data;
    }

    public bool Loaded { get; }
    public StartupQueryDto Data { get; }
}
