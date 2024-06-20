namespace mark.davison.common.client.web.abstractions.Store;

public class BaseAction
{
    public Guid ActionId { get; set; } = Guid.NewGuid();
}
