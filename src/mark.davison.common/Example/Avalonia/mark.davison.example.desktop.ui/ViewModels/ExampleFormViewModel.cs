namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleFormViewModel : ObservableObject, IFormViewModel
{
    public bool Valid => true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Valid))]
    private bool _triggerErrorOnSubmit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Valid))]
    private decimal? _errorsToTrigger = 1;

}

public class ExampleFormViewModelSubmission : IFormSubmission<ExampleFormViewModel>
{
    public async Task<Response> Primary(ExampleFormViewModel formViewModel)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));

        if (formViewModel.TriggerErrorOnSubmit)
        {
            var response = new Response();

            for (int i = 0; i < Math.Max(1, (int)formViewModel.ErrorsToTrigger.GetValueOrDefault(1)); ++i)
            {
                response.Errors.Add($"Error #{i}");
            }

            return response;
        }

        return new Response();
    }
}
