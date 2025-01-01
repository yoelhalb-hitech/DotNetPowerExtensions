
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IFieldDetail : IMemberDetail<IFieldDetail>
{
    ITypeDetailInfo FieldType { get; }
}
