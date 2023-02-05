﻿namespace mark.davison.common.server.tests.CQRS;

[TestClass]
public class ValidateAndProcessQueryHandlerTests
{
    private class TestHandler : ValidateAndProcessQueryHandler<ExampleQueryRequest, ExampleQueryResponse>
    {
        public TestHandler(
            IQueryProcessor<ExampleQueryRequest, ExampleQueryResponse> processor
        ) : base(
            processor
        )
        {
        }

        public TestHandler(
            IQueryProcessor<ExampleQueryRequest, ExampleQueryResponse> processor,
            IQueryValidator<ExampleQueryRequest, ExampleQueryResponse> validator
        ) : base(
            processor,
            validator
        )
        {
        }
    }

    private readonly Mock<IQueryProcessor<ExampleQueryRequest, ExampleQueryResponse>> _processor;
    private readonly Mock<IQueryValidator<ExampleQueryRequest, ExampleQueryResponse>> _validator;
    private readonly Mock<ICurrentUserContext> _userContext;

    public ValidateAndProcessQueryHandlerTests()
    {
        _processor = new();
        _validator = new();
        _userContext = new();
    }

    [TestMethod]
    public async Task Handle_WithValidator_InvokesValidator()
    {
        var handler = new TestHandler(_processor.Object, _validator.Object);

        _validator
            .Setup(_ => _.ValidateAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse())
            .Verifiable();

        await handler.Handle(new(), _userContext.Object, CancellationToken.None);

        _validator.VerifyAll();
    }

    [TestMethod]
    public async Task Handle_WithoutValidator_DoesNotInvokeValidator()
    {
        var handler = new TestHandler(_processor.Object);

        _validator
            .Setup(_ => _.ValidateAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse())
            .Verifiable();

        await handler.Handle(new(), _userContext.Object, CancellationToken.None);

        _validator
            .Verify(_ =>
                _.ValidateAsync(
                    It.IsAny<ExampleQueryRequest>(),
                    It.IsAny<ICurrentUserContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithoutValidator_InvokesProcessor()
    {
        var handler = new TestHandler(_processor.Object);

        _processor
            .Setup(_ => _.ProcessAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse())
            .Verifiable();

        await handler.Handle(new(), _userContext.Object, CancellationToken.None);

        _processor.VerifyAll();
    }

    [TestMethod]
    public async Task Handle_WithValidator_InvokesProcessor_IfValidationPasses()
    {
        var handler = new TestHandler(_processor.Object, _validator.Object);

        _validator
            .Setup(_ => _.ValidateAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse());

        _processor
            .Setup(_ => _.ProcessAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse())
            .Verifiable();

        await handler.Handle(new(), _userContext.Object, CancellationToken.None);

        _processor.VerifyAll();
    }

    [TestMethod]
    public async Task Handle_WithValidator_DoesNotInvokeProcessor_IfValidationFails()
    {
        var handler = new TestHandler(_processor.Object, _validator.Object);

        _validator
            .Setup(_ => _.ValidateAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse { Errors = new() { "ERROR " } });

        _processor
            .Setup(_ => _.ProcessAsync(
                It.IsAny<ExampleQueryRequest>(),
                It.IsAny<ICurrentUserContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExampleQueryResponse())
            .Verifiable();

        await handler.Handle(new(), _userContext.Object, CancellationToken.None);

        _processor
            .Verify(_ =>
                _.ProcessAsync(
                    It.IsAny<ExampleQueryRequest>(),
                    It.IsAny<ICurrentUserContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
