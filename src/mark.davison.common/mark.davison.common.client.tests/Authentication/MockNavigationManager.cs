namespace mark.davison.common.client.web.tests.Authentication;

// TODO: Move to test assembly
public class MockNavigationManager : NavigationManager, IDisposable
{
    public string? NavigateToLocation { get; private set; }

    public new virtual void NavigateTo(string uri, bool forceLoad = false)
    {
        NavigateToCore(uri, forceLoad);
    }
    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        NavigateToLocation = uri;
        Uri = $"{this.BaseUri}{uri}";
    }

    protected sealed override void EnsureInitialized()
    {
        Initialize("http://localhost:5000/", "http://localhost:5000/dashboard/");
    }

    public MockNavigationManager(string baseUri, string uri)
    {
        Initialize(baseUri, uri);
    }

    public MockNavigationManager()
    {
        EnsureInitialized();
    }

    public void Initialise(string baseUri, string uri)
    {
        Initialize(baseUri, uri);
    }


    public void Dispose()
    {
    }
}