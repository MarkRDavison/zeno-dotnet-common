using mark.davison.common.client.abstractions.Form;
using mark.davison.common.CQRS;
using mark.davison.common.source.generators.Form;

namespace mark.davison.common.source.generators.test.Form;

[TestClass]
public sealed class FormSubmissionSourceGeneratorTests
{
    [TestMethod]
    public void TestFormSubmissionGeneration()
    {
        var source = @"
using mark.davison.common.CQRS;
using mark.davison.common.client.abstractions.Form;
using mark.davison.common.source.generators.client;
using System.Threading.Tasks;

namespace mark.davison.tests.web
{
    [UseForm]
    public sealed class WebRoot 
    {
    }
}

namespace mark.davison.tests.forms
{

    public partial class ExampleFormViewModel : IFormViewModel
    {
        public bool Valid => true;
    }

    public sealed class ExampleFormViewModelSubmission : IFormSubmission<ExampleFormViewModel>
    {

        public async Task<Response> Primary(ExampleFormViewModel formViewModel)
        {
            await Task.CompletedTask;
            return new Response();
        }

    }

}
";

        var result = TestHelper.RunSourceGenerator<FormSubmissionSourceGenerator>(
            source,
            [
                typeof(IFormViewModel),
                typeof(IFormSubmission<>),
                typeof(Response)
            ]);

        Assert.IsLessThanOrEqualTo(1, result.Diagnostics.Length);

        var expectedHintNameDependencyInjection = "FormSubmissionDependecyInjectionExtensions.g.cs";

        Assert.HasCount(1, result.Results);
        Assert.IsTrue(result.Results.First().GeneratedSources.Any(_ => _.HintName == expectedHintNameDependencyInjection));

        var di = result.Results
            .First()
            .GeneratedSources
            .First(_ => _.HintName == expectedHintNameDependencyInjection);

        var sourceStringDi = di.SourceText.ToString();

        Assert.IsFalse(string.IsNullOrEmpty(sourceStringDi));

        Assert.Fail();// TODO: ADD ASSERTIONS
    }
}
