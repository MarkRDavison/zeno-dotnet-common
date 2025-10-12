namespace mark.davison.common.source.generators.Form;

public sealed class FormSubmissionInfo
{
    public FormSubmissionInfo(
        string formSubmissionInterface, 
        string formSubmissionImplementation,
        string formViewModel,
        string rootNamespace)
    {
        FormSubmissionInterface = formSubmissionInterface;
        FormSubmissionImplementation = formSubmissionImplementation;
        FormViewModel = formViewModel;
        RootNamespace = rootNamespace;
    }

    public string FormSubmissionInterface { get; }
    public string FormSubmissionImplementation { get; }
    public string FormViewModel { get; }
    public string RootNamespace { get; }
}
