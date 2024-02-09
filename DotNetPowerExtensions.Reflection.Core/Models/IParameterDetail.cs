
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IParameterDetail
{
    string Name { get; }
    ITypeDetailInfo ParameterType { get; }
    ParameterModifierTypes ParameterModifierType { get; }
}
