namespace mark.davison.common.desktop.components.Models;

public sealed class PersistedAuthentication
{
    public string Username { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
