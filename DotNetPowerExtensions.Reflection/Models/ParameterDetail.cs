
namespace SequelPay.DotNetPowerExtensions.Reflection;

public class ParameterDetail : IParameterDetail
{
    internal ParameterDetail() { }

    [MustInitialize] public string Name { get; internal set; }
    [MustInitialize] public TypeDetailInfo ParameterType { get; internal set; }
    [MustInitialize] public ParameterModifierTypes ParameterModifierType { get; internal set; }

    ITypeDetailInfo IParameterDetail.ParameterType => ParameterType;
}
