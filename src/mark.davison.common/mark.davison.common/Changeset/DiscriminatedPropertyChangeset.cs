namespace mark.davison.common.Changeset;

public class DiscriminatedPropertyChangeset
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string PropertyType { get; set; } = string.Empty;
}
