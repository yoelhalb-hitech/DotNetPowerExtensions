using SequelPay.DotNetPowerExtensions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Models;

public class PropertyDetail : MemberDetail<PropertyInfo>
{
	internal PropertyDetail(){}

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Property; internal set => throw new NotSupportedException(); }

    [MustInitialize] public FieldDetail? BackingField { get; internal set; }
    [MustInitialize] public MethodDetail? GetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? SetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? BasePrivateGetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? BasePrivateSetMethod { get; internal set; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public PropertyDetail? OverridenProperty { get; internal set; }
}
