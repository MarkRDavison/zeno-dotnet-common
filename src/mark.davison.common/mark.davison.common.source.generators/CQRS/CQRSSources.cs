﻿namespace mark.davison.common.source.generators.CQRS;

[ExcludeFromCodeCoverage]
public static class CQRSSources
{
    public static string UseCQRSServerAttribute(string ns)
    {
        return $@"namespace {ns};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseCQRSServerAttribute : Attribute
{{
    public Type[] Types {{ get; set; }}

    public UseCQRSServerAttribute(params Type[] types)
    {{
        Types = types;
    }}
}}";
    }
    public static string UseCQRSClientAttribute(string ns)
    {
        return $@"namespace {ns};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseCQRSClientAttribute : Attribute
{{
    public Type[] Types {{ get; set; }}

    public UseCQRSClientAttribute(params Type[] types)
    {{
        Types = types;
    }}
}}";
    }
}
