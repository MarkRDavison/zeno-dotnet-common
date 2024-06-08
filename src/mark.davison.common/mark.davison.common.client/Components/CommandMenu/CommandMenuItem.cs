namespace mark.davison.common.client.Components.CommandMenu;

public class CommandMenuItem
{
    public string Text { get; set; } = string.Empty;
    public string? Id { get; set; }
    public List<CommandMenuItem>? Children { get; set; }
}
