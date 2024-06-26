namespace mark.davison.common.client.desktop.components.Services;

public class PageEventArgs : EventArgs
{
    public PageEventArgs(string groupId, string pageId)
    {
        GroupId = groupId;
        PageId = pageId;
    }

    public string GroupId { get; }
    public string PageId { get; }
}
public class ChangePageEventArgs : PageEventArgs
{
    public ChangePageEventArgs(string groupId, string pageId) : base(groupId, pageId) { }
}
public class ClosePageEventArgs : PageEventArgs
{
    public ClosePageEventArgs(string groupId, string pageId) : base(groupId, pageId) { }
}

public interface ICommonApplicationNotificationService
{
    event EventHandler AuthenticationStateChanged;
    void NotifyAuthenticationStateChanged();

    event EventHandler<ChangePageEventArgs> PageChanged;
    void ChangePage(string groupId, string pageId);

    event EventHandler<ClosePageEventArgs> PageClosed;
    void ClosePage(string groupId, string pageId);

    event EventHandler PageEnabledStateChanged;
    void NotifyPageEnabledStateChanged();
}
