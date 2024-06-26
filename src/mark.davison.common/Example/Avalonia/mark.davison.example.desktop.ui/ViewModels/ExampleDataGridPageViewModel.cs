namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleDataGridPageViewModel : BasicApplicationPageViewModel
{
    public ExampleDataGridPageViewModel(
        ICommonApplicationNotificationService commonApplicationNotificationService
    ) : base(
        commonApplicationNotificationService)

    {
        DataGridRowItems.Add(new DataGridRowItem { FirstName = "John", LastName = "Doe", Age = 52 });
        DataGridRowItems.Add(new DataGridRowItem { FirstName = "Jane", LastName = "Doe", Age = 22 });
    }

    public override string Name => "Data grid";
    public override bool Disabled => false;
    public ObservableCollection<DataGridRowItem> DataGridRowItems { get; } = [];
}
