namespace mark.davison.common.persistence.tests.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid BlogId { get; set; }
    public virtual Blog? Blog { get; set; }
}
