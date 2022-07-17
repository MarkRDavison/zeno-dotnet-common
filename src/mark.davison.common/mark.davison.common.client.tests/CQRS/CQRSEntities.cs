namespace mark.davison.common.client.tests.CQRS;


public class ExampleCommandRequest : ICommand<ExampleCommandRequest, ExampleCommandResponse>
{

}

public class ExampleCommandResponse
{

}

public class ExampleCommandHandler : ICommandHandler<ExampleCommandRequest, ExampleCommandResponse>
{
    public Task<ExampleCommandResponse> Handle(ExampleCommandRequest command, CancellationToken cancellation)
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
    public Task<ExampleQueryResponse> Handle(ExampleQueryRequest query, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleQueryResponse());
    }
}

public class ExampleAction : IAction<ExampleAction>
{

}

public class ExampleActionHandler : IActionHandler<ExampleAction>
{
    public Task Handle(ExampleAction query, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}