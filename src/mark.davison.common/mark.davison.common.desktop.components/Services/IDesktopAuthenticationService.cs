namespace mark.davison.common.desktop.components.Services;

public interface IDesktopAuthenticationService
{
    bool IsAuthenticated { get; }
    string Username { get; }
    Task<(bool, string)> LoginAsync(CancellationToken cancellationToken);
    Task<(bool, string)> LoginFromPersisted();
    void PersistLogin();
    void Logout();
}
