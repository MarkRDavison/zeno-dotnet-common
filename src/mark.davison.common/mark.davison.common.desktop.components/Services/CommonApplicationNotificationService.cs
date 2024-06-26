namespace mark.davison.common.client.desktop.components.Services;

public class CommonApplicationNotificationService : ICommonApplicationNotificationService
{
    public event EventHandler AuthenticationStateChanged = default!;

    public void NotifyAuthenticationStateChanged()
    {
        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ChangePageEventArgs> PageChanged = default!;
    public void ChangePage(string groupId, string pageId)
    {
        PageChanged?.Invoke(this, new ChangePageEventArgs(groupId, pageId));
    }

    public event EventHandler<ClosePageEventArgs> PageClosed = default!;
    public void ClosePage(string groupId, string pageId)
    {
        PageClosed?.Invoke(this, new ClosePageEventArgs(groupId, pageId));
    }

    public event EventHandler PageEnabledStateChanged = default!;
    public void NotifyPageEnabledStateChanged()
    {
        PageEnabledStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
