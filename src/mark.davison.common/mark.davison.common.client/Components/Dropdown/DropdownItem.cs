namespace mark.davison.common.client.Components;

public class DropdownItem : IDropdownItem
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}
