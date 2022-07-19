namespace mark.davison.common.server.abstractions.Identification;

public class User : BaseEntity
{
    public Guid Sub { get; set; }
    public string Username { get; set; } = string.Empty;
    public string First { get; set; } = string.Empty;
    public string Last { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Admin { get; set; }
}
