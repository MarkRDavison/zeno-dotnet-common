using Microsoft.AspNetCore.Components.Forms;

namespace mark.davison.common.client.web.Components;

public partial class FileUploadDialog<T> where T : class, new()
{

    private bool _inProgress;
    public bool _primaryDisabled => _inProgress || data == null;

    private T? data;

    [CascadingParameter, EditorRequired]
    public required IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string PrimaryText { get; set; } = "Ok";

    [Parameter, EditorRequired]
    public required Func<T, Task<Response>> PrimaryCallback { get; set; }

    [Parameter, EditorRequired]
    public required Func<T, string> CorrectFileDescriptionCallback { get; set; }

    [Parameter, EditorRequired]
    public required Color Color { get; set; }

    [Parameter,]
    public string? InvalidFormatSnackbarMessage { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }


    private async Task OnInputFileChanged(InputFileChangeEventArgs args)
    {
        data = null;
        var files = args.GetMultipleFiles();

        var file = files.FirstOrDefault();
        if (file == null)
        {
            return;
        }

        using (var stream = file.OpenReadStream())
        {
            try
            {
                data = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions());
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Imported file was not in correct format\nDid you select the correct file?");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                if (!string.IsNullOrEmpty(InvalidFormatSnackbarMessage))
                {
                    Snackbar.Add(InvalidFormatSnackbarMessage, Severity.Error);
                }
            }
        }
    }

    private async Task Primary()
    {
        _inProgress = true;

        if (data == null)
        {
            return;
        }

        var response = await PrimaryCallback(data);

        if (response.Success)
        {
            MudDialog.Close(DialogResult.Ok(response));
            data = null;
        }
        else
        {
            Console.Error.WriteLine("TODO: How to handle errors in FileUploadDialog.Primary, add to message bar/toast???");
            response.Errors.ForEach(Console.Error.WriteLine);
            response.Warnings.ForEach(Console.WriteLine);
        }

        _inProgress = false;
    }

    private void Secondary()
    {
        data = null;
        MudDialog.Cancel();
    }

    private string GetParsedInputDisplayText() => data == null ? string.Empty : CorrectFileDescriptionCallback(data);
}
