using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Reflection.Models;

public class MethodDetail : MemberDetail<MethodInfo>
{
    internal MethodDetail() { }

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Method; internal set => throw new NotSupportedException(); }
    [MustInitialize] public Type[] ArgumentTypes { get; internal set; }
    [MustInitialize] public Type ReturnType { get; internal set; }
    [MustInitialize] public Type[] GenericArguments { get; internal set; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public MethodDetail? OverridenMethod { get; internal set; }
}
