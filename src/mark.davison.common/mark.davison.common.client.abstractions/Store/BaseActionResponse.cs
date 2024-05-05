namespace mark.davison.common.client.abstractions.Store;

public class BaseActionResponse : Response
{
    public Guid ActionId { get; set; }
}

public class BaseActionResponse<T> : BaseActionResponse
{
    public BaseActionResponse()
    {

    }
    public BaseActionResponse(BaseAction action)
    {
        ActionId = action.ActionId;
    }

    [MemberNotNullWhen(returnValue: true, nameof(Response<T>.Value))]
    public bool SuccessWithValue => Success && Value != null;
    public T? Value { get; set; }
}
