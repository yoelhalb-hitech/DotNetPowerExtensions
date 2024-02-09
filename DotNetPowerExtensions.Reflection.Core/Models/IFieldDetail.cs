
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IFieldDetail : IMemberDetail<IFieldDetail>
{
    ITypeDetailInfo FieldType { get; }
}
