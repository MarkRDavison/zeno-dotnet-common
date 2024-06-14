namespace mark.davison.example.shared.models;

public class ExampleEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
}
