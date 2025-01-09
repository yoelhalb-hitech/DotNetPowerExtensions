
namespace SequelPay.DotNetPowerExtensions.Reflection;

public record PropertyDetail : MemberDetail<PropertyInfo, PropertyDetail, IPropertyDetail>, IPropertyDetail
{
	internal PropertyDetail(){}

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Property; internal set => throw new NotSupportedException(); }

    public Type PropertyType => ReflectionInfo.PropertyType;

    [MustInitialize] public FieldDetail? BackingField { get; internal set; }
    [MustInitialize] public MethodDetail? GetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? SetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? BasePrivateGetMethod { get; internal set; }
    [MustInitialize] public MethodDetail? BasePrivateSetMethod { get; internal set; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public PropertyDetail? OverridenProperty { get; internal set; }

    ITypeDetailInfo IPropertyDetail.PropertyType => PropertyType.GetTypeDetailInfo();
    IFieldDetail? IPropertyDetail.BackingField => BackingField;
    IMethodDetail? IPropertyDetail.GetMethod => GetMethod;
    IMethodDetail? IPropertyDetail.SetMethod => SetMethod;
    IMethodDetail? IPropertyDetail.BasePrivateGetMethod => BasePrivateGetMethod;
    IMethodDetail? IPropertyDetail.BasePrivateSetMethod => BasePrivateSetMethod;
    IPropertyDetail? IPropertyDetail.OverridenProperty => OverridenProperty;
}
