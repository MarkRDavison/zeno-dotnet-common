namespace mark.davison.common.source.generators.CQRS;

public static class CQRSSources
{
    public static string UseCQRSAttribute(string ns)
    {
        return $@"namespace {ns};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseCQRSAttribute : Attribute
{{
    public Type[] Types {{ get; set; }}

    public UseCQRSAttribute(params Type[] types)
    {{
        Types = types;
    }}
}}";
    }
}
