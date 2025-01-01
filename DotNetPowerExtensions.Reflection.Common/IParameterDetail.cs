
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IParameterDetail
{
    string Name { get; }
    ITypeDetailInfo ParameterType { get; }
    ParameterModifierTypes ParameterModifierType { get; }
}
