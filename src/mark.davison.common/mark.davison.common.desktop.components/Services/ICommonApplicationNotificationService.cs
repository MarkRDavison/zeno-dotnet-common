namespace mark.davison.common.client.desktop.components.Services;

public class ChangePageEventArgs : EventArgs
{
    public ChangePageEventArgs(string groupId, string pageId)
    {
        GroupId = groupId;
        PageId = pageId;
    }

    public string GroupId { get; }
    public string PageId { get; }
}

public interface ICommonApplicationNotificationService
{
    event EventHandler AuthenticationStateChanged;
    void NotifyAuthenticationStateChanged();
    event EventHandler<ChangePageEventArgs> PageChanged;
    void ChangePage(string groupId, string pageId);
}
