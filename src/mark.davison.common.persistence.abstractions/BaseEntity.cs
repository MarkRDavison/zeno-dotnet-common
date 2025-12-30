namespace mark.davison.common.persistence;

public class BaseEntity
{
    public required Guid Id { get; set; }
    public required DateTimeOffset Created { get; set; }
    public required DateTimeOffset LastModified { get; set; }
    public required Guid UserId { get; set; }
}
