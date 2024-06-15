namespace mark.davison.common.desktop.components.Services;

public interface ICommonApplicationNotificationService
{
    event EventHandler AuthenticationStateChanged;
    void NotifyAuthenticationStateChanged();
}
