namespace mark.davison.common.desktop.components.Models;

public sealed class OdicClientSettings
{
    public OdicClientSettings()
    {

    }
    public required string PersistenceLocation { get; set; }
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string Scope { get; set; }
}
