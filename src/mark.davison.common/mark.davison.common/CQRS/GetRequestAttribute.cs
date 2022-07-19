namespace mark.davison.common.CQRS;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GetRequestAttribute : Attribute
{
    public string Path { get; set; } = null!;
}
