namespace mark.davison.common.server.tests.CQRS;

public class ExampleCommandRequest : ICommand<ExampleCommandRequest, ExampleCommandResponse>
{

}

public class ExampleCommandResponse
{

}

public class ExampleCommandHandler : ICommandHandler<ExampleCommandRequest, ExampleCommandResponse>
{
    public Task<ExampleCommandResponse> Handle(ExampleCommandRequest command, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleCommandResponse());
    }
}

public class ExampleQueryRequest : IQuery<ExampleQueryRequest, ExampleQueryResponse>
{

}

public class ExampleQueryResponse
{

}

public class ExampleQueryHandler : IQueryHandler<ExampleQueryRequest, ExampleQueryResponse>
{
    public Task<ExampleQueryResponse> Handle(ExampleQueryRequest query, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleQueryResponse());
    }
}