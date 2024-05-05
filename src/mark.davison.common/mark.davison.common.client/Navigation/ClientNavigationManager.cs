namespace mark.davison.common.client.Navigation;

public class ClientNavigationManager : IClientNavigationManager
{
    private readonly NavigationManager _navigationManager;

    public ClientNavigationManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NavigateTo(string url)
    {
        _navigationManager.NavigateTo(url);
    }
}
