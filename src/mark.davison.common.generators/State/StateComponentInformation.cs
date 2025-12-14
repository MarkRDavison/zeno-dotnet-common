namespace mark.davison.common.generators.State;

public sealed class StateForComponentInformation
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
}

public sealed class StateComponentInformation
{
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentNamespace { get; set; } = string.Empty;
    public List<StateForComponentInformation> States { get; set; } = [];
}
