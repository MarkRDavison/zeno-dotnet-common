namespace mark.davison.common.server.abstractions.Authentication;

public interface ICurrentUserContext
{
    public User CurrentUser { get; set; }
    public string Token { get; set; }
}