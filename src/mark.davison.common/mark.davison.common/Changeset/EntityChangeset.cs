namespace mark.davison.common.Changeset;

public class EntityChangeset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntityId { get; set; }
    public string Type { get; set; } = string.Empty;
    public EntityChangeType EntityChangeType { get; set; }
    public List<PropertyChangeset> PropertyChangesets { get; set; } = new();
}
