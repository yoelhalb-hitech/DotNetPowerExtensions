using SequelPay.DotNetPowerExtensions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Models;

public class MemberDetail<T> where T : MemberInfo
{
    internal MemberDetail() { }

    [MustInitialize] public string Name { get; internal set; }
    [MustInitialize] public T ReflectionInfo { get; internal set; }
    [MustInitialize] public virtual MemberDetailTypes MemberDetailType { get; internal set; }
    [MustInitialize] public bool IsInherited { get; internal set; }
    [MustInitialize] public DeclarationTypes DeclarationType { get; internal set; }
    [MustInitialize] public bool InReflectionForCurrentType { get; internal set; }
    [MustInitialize] public bool IsExplicit { get; internal set; }
    [MustInitialize] public Type? ExplicitInterface { get; internal set; }
    [MustInitialize] public T? ExplicitInterfaceReflectionInfo { get; internal set; }
}
