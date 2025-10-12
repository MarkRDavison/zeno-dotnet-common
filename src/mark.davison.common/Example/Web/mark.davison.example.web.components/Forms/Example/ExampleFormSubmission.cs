namespace mark.davison.example.web.components.Forms.Example;

public sealed class ExampleFormSubmission : IFormSubmission<ExampleFormViewModel>
{
    public Task<Response> Primary(ExampleFormViewModel formViewModel)
    {
        return Task.FromResult(new Response());
    }
}
