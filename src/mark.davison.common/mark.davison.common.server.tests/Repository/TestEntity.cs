namespace mark.davison.common.server.tests.Repository;

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
}
