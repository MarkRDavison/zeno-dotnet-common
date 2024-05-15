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
