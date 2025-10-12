namespace mark.davison.common.source.generators.Form;

[ExcludeFromCodeCoverage]
public static class FormSources
{
    public static string UseFormAttribute(string ns)
    {
        return $@"
using System;

namespace {ns};

//[global::Microsoft.CodeAnalysis.EmbeddedAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseFormAttribute : Attribute
{{
}}";
    }
}
