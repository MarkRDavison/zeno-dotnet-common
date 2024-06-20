namespace mark.davison.common.client.web.abstractions.Store;

public sealed class StoreHelperOptions
{
    public bool VerboseActionIdComparison { get; set; }
    public Func<object, object, bool>? ResponseCompareFunction { get; set; }
}
