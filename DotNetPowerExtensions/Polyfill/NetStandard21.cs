#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited = false)]
    internal sealed class DisallowNullAttribute : Attribute { }
}

#endif
