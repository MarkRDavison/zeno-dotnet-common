namespace mark.davison.common.server.Authentication;

[ExcludeFromCodeCoverage]
public class CurrentUserContext : ICurrentUserContext
{
    public User CurrentUser { get; set; } = default!;
    public string Token { get; set; } = default!;
}