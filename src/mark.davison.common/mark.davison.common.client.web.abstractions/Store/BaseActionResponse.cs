namespace mark.davison.common.client.web.abstractions.Store;

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

    // TODO: This is bad, not returning the expected type
    public static BaseActionResponse<T> From(Response response)
    {
        return new BaseActionResponse<T>
        {
            Errors = response.Errors,
            Warnings = response.Warnings
        };
    }

    [MemberNotNullWhen(returnValue: true, nameof(Response<T>.Value))]
    public bool SuccessWithValue => Success && Value != null;
    public T? Value { get; set; }
}
