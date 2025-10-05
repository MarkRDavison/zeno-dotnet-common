using System;

namespace mark.davison.common.CQRS;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PostRequestAttribute : Attribute
{
    public string Path { get; set; } = null!;
    public bool AllowAnonymous { get; set; }
}