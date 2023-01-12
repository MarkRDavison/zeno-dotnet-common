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

public class ExampleResponseActionRequest : IResponseAction<ExampleResponseActionRequest, ExampleResponseActionResponse>
{

}

public class ExampleResponseActionResponse
{

}

public class ExampleActionResponseHandler : IResponseActionHandler<ExampleResponseActionRequest, ExampleResponseActionResponse>
{
    public Task<ExampleResponseActionResponse> Handle(ExampleResponseActionRequest query, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleResponseActionResponse());
    }
}

public interface IListStateItem
{

}

public class ListStateItem : IListStateItem
{

}

public class FetchListStateAction<TListStateItem> : IAction<FetchListStateAction<TListStateItem>>
    where TListStateItem : class, IListStateItem
{
}

public abstract class FetchListStateActionHandler<TListStateItem> : IActionHandler<FetchListStateAction<TListStateItem>>
    where TListStateItem : class, IListStateItem
{
    public Task Handle(FetchListStateAction<TListStateItem> action, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}

public class FetchImplementationListStateActionHandler : FetchListStateActionHandler<ListStateItem> { }

public class ListStateCommandResponse<TListStateItem> : Response<List<TListStateItem>> { }
public class ListStateCommandRequest<TListStateItem> : ICommand<ListStateCommandRequest<TListStateItem>, ListStateCommandResponse<TListStateItem>> { }

public abstract class ListStateCommandHandler<TListStateItem> : ICommandHandler<ListStateCommandRequest<TListStateItem>, ListStateCommandResponse<TListStateItem>>
{
    public Task<ListStateCommandResponse<TListStateItem>> Handle(ListStateCommandRequest<TListStateItem> command, CancellationToken cancellation)
    {
        return Task.FromResult(new ListStateCommandResponse<TListStateItem>());
    }
}

public class ListStateCommandHandlerImplementation : ListStateCommandHandler<ListStateItem>
{

}

public class ListStateQueryResponse<TListStateItem> : Response<List<TListStateItem>> { }
public class ListStateQueryRequest<TListStateItem> : IQuery<ListStateQueryRequest<TListStateItem>, ListStateQueryResponse<TListStateItem>> { }

public abstract class ListStateQueryHandler<TListStateItem> : IQueryHandler<ListStateQueryRequest<TListStateItem>, ListStateQueryResponse<TListStateItem>>
{
    public Task<ListStateQueryResponse<TListStateItem>> Handle(ListStateQueryRequest<TListStateItem> command, CancellationToken cancellation)
    {
        return Task.FromResult(new ListStateQueryResponse<TListStateItem>());
    }
}

public class ListStateQueryHandlerImplementation : ListStateQueryHandler<ListStateItem>
{

}

public class ListStateResponseActionResponse<TListStateItem> { }
public class ListStateResponseActionRequest<TListStateItem> : IResponseAction<ListStateResponseActionRequest<TListStateItem>, ListStateResponseActionResponse<TListStateItem>> { }

public abstract class ListStateResponseActionHandler<TListStateItem> : IResponseActionHandler<ListStateResponseActionRequest<TListStateItem>, ListStateResponseActionResponse<TListStateItem>>
{
    public Task<ListStateResponseActionResponse<TListStateItem>> Handle(ListStateResponseActionRequest<TListStateItem> action, CancellationToken cancellation)
    {
        return Task.FromResult(new ListStateResponseActionResponse<TListStateItem>());
    }
}
public class ListStateResponseActionHandlerImplementation : ListStateResponseActionHandler<ListStateItem>
{

}