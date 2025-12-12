namespace mark.davison.common.generators.State;

public sealed class EffectMethodInformation
{
    public EffectMethodInformation(string actionType, string memberName)
    {
        ActionType = actionType;
        MemberName = memberName;
    }

    public string ActionType { get; }
    public string MemberName { get; }
}
public sealed class ReducerMethodInformation
{
    public ReducerMethodInformation(string stateType, string action, string memberName)
    {
        StateType = stateType;
        Action = action;
        MemberName = memberName;
    }

    public string StateType { get; }
    public string Action { get; }
    public string MemberName { get; }
}

public sealed class FeatureInformation
{
    public FeatureInformation(
        string name,
        string @namespace,
        FeatureType type,
        ImmutableArray<EffectMethodInformation> effectMethods,
        ImmutableArray<ReducerMethodInformation> reducerMethods,
        List<string> attributes)
    {
        Name = name;
        Namespace = @namespace;
        Type = type;
        EffectMethods = effectMethods;
        ReducerMethods = reducerMethods;
        Attributes = attributes;
    }

    public string Name { get; }
    public string Namespace { get; }
    public FeatureType Type { get; }
    public ImmutableArray<EffectMethodInformation> EffectMethods { get; }
    public ImmutableArray<ReducerMethodInformation> ReducerMethods { get; }
    public List<string> Attributes { get; }
};