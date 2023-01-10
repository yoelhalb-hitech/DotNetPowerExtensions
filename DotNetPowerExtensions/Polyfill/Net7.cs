#if !NET7_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    [System.AttributeUsage(System.AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}

namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute { }
}

#endif