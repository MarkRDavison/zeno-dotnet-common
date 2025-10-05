using mark.davison.common.CQRS;
using mark.davison.common.server.abstractions.Authentication;
using mark.davison.common.server.abstractions.CQRS;
using mark.davison.common.server.CQRS.Processors;
using mark.davison.common.server.CQRS.Validators;
using mark.davison.common.source.generators.test.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.Serialization;

namespace mark.davison.common.source.generators.test;

[TestClass]
public sealed class IncrementalCQRSGeneratorTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var source = @"
using System.Threading;
using System.Threading.Tasks;
using mark.davison.common.CQRS;
using mark.davison.common.source.generators.CQRS;
using mark.davison.common.server.abstractions.CQRS;
using mark.davison.common.server.CQRS.Processors;
using mark.davison.common.server.CQRS.Validators;
using mark.davison.common.server.abstractions.Authentication;
using System.Diagnostics.CodeAnalysis;

namespace mark.davison.tests.api
{
    [UseCQRSServer(typeof(ApiRoot))]
    public class ApiRoot
    {
    }
}

namespace mark.davison.tests.shared
{
    [ExcludeFromCodeCoverage]
    [PostRequest(Path = ""test-command"")]
    public sealed class TestCommand : ICommand<TestCommand, TestCommandResponse>
    {

    }

    public sealed class TestCommandResponse : Response
    {

    }

    public sealed class TestCommandHandler : ICommandHandler<TestCommand, TestCommandResponse>
    {
        public async Task<TestCommandResponse> Handle(TestCommand command, ICurrentUserContext currentUserContext, CancellationToken cancellation)
        {
            return await Task.FromResult(new TestCommandResponse());
        }
    }

    public sealed class TestCommandProcessor : ICommandProcessor<TestCommand, TestCommandResponse>
    {
        public async Task<TestCommandResponse> ProcessAsync(TestCommand request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new TestCommandResponse());
        }
    }

    public sealed class TestCommandValidator : ICommandValidator<TestCommand, TestCommandResponse>
    {
        public async Task<TestCommandResponse> ValidateAsync(TestCommand request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new TestCommandResponse());
        }
    }

    public sealed class TestQuery : IQuery<TestQuery, TestQueryResponse>
    {

    }

    public sealed class TestQueryResponse : Response
    {

    }

    public sealed class TestQueryHandler : IQueryHandler<TestQuery, TestQueryResponse>
    {
        public async Task<TestQueryResponse> Handle(TestQuery Query, ICurrentUserContext currentUserContext, CancellationToken cancellation)
        {
            return await Task.FromResult(new TestQueryResponse());
        }
    }

    public sealed class TestQueryProcessor : IQueryProcessor<TestQuery, TestQueryResponse>
    {
        public async Task<TestQueryResponse> ProcessAsync(TestQuery request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new TestQueryResponse());
        }
    }

    public sealed class TestQueryValidator : IQueryValidator<TestQuery, TestQueryResponse>
    {
        public async Task<TestQueryResponse> ValidateAsync(TestQuery request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new TestQueryResponse());
        }
    }
}
";

        var result = TestHelper.RunSourceGenerator<IncrementalCQRSGenerator>(
            source,
            [
                typeof(Object),
                typeof(GCSettings),
                typeof(CQRSType), 
                typeof(ICommand<,>),
                typeof(IQuery<,>), 
                typeof(ICommandHandler<,>), 
                typeof(ICommandProcessor<,>),
                typeof(ICommandValidator<,>),
                typeof(PostRequestAttribute),
                typeof(GetRequestAttribute),
                typeof(SourceGeneratorHelpers)
            ]);

        var expectedHintName = "CQRSServerDependecyInjectionExtensions.g.cs";

        Assert.HasCount(1, result.Results);
        Assert.IsTrue(result.Results.First().GeneratedSources.Any(_ => _.HintName == expectedHintName));

        var ignitionsource = result.Results
            .First()
            .GeneratedSources
            .First(_ => _.HintName == expectedHintName);

        var sourceString = ignitionsource.SourceText.ToString();
    }
}