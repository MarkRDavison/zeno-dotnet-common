namespace mark.davison.common.server.abstractions;

public class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
}
