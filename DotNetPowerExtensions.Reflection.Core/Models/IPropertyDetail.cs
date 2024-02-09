
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IPropertyDetail : IMemberDetail<IPropertyDetail>
{
    ITypeDetailInfo PropertyType { get; }
    IFieldDetail? BackingField { get; }
    IMethodDetail? GetMethod { get; }
    IMethodDetail? SetMethod { get; }
    IMethodDetail? BasePrivateGetMethod { get; }
    IMethodDetail? BasePrivateSetMethod { get; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    IPropertyDetail? OverridenProperty { get; }
}
