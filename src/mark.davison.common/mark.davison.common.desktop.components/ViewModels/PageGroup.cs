namespace mark.davison.common.client.desktop.ViewModels;

public partial class PageGroup : ObservableObject
{
    public PageGroup(
        string name,
        IEnumerable<BasicApplicationPageViewModel> pages,
        string? id = null)
    {
        Id = id ?? name;
        Name = name;
        SubPages = [.. pages];
    }

    public string Id { get; }
    public string Name { get; }
    public ObservableCollection<BasicApplicationPageViewModel> SubPages { get; }

    public BasicApplicationPageViewModel SubPage => SubPages[SelectedIndex];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubPage))]
    private int _selectedIndex;

    [RelayCommand]
    private void SelectSubPage(BasicApplicationPageViewModel page)
    {
        SelectedIndex = SubPages.IndexOf(page);
        page.Select();
    }

    public bool Single => SubPages.Count == 1;

    internal void NotifyDisabledChange()
    {
        OnPropertyChanged(nameof(Disabled));
        OnPropertyChanged(nameof(SubPages));
    }

    public bool Disabled => SubPages.Any(x => x.Disabled);
}
