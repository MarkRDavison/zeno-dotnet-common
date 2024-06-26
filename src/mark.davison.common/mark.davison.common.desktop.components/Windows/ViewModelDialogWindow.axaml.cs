using mark.davison.common.client.desktop.ViewModels;

namespace mark.davison.common.client.desktop.components;

public partial class ViewModelDialogWindow : Window
{
    public ViewModelDialogWindow()
    {
        InitializeComponent();

        ClearErrorCommand = new RelayCommand<string>(ClearError);
    }

    public IRelayCommand<string> ClearErrorCommand { get; }

    private void ClearError(string? error)
    {
        if (error is not null && Errors.Contains(error))
        {
            Errors.Remove(error);
        }
    }

    private void Cancel_Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();

    public ObservableCollection<string> Errors { get; } = [];

    private async void PrimaryButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ViewModelDialogViewModel vm)
        {
            IsEnabled = false;
            Errors.Clear();
            var result = await vm.Primary();

            IsEnabled = true;
            if (result.Success)
            {
                Close(result);
            }
            else
            {
                foreach (var err in result.Errors)
                {
                    Errors.Add(err);
                }
            }
        }
    }
}