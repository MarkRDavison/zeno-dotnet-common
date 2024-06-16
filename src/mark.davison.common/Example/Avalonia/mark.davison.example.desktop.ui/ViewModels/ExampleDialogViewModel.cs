namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleDialogViewModel : ObservableObject, IViewModelDialogViewModel
{
    public string Title => "Example dialog";

    public bool IsValid => true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private bool _triggerErrorOnSubmit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private decimal? _errorsToTrigger = 1;

    public async Task<Response> PrimaryCallback()
    {
        await Task.Delay(TimeSpan.FromSeconds(3));

        if (TriggerErrorOnSubmit)
        {
            var response = new Response();

            for (int i = 0; i < Math.Max(1, (int)ErrorsToTrigger.GetValueOrDefault(1)); ++i)
            {
                response.Errors.Add($"Error #{i}");
            }

            return response;
        }

        return new Response();
    }
}
