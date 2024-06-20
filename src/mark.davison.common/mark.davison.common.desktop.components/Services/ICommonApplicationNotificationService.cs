namespace mark.davison.common.client.desktop.components.Services;

public interface ICommonApplicationNotificationService
{
    event EventHandler AuthenticationStateChanged;
    void NotifyAuthenticationStateChanged();
}
