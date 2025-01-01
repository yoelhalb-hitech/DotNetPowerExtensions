
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IMethodBase<TMethodDetail> : IMemberDetail<TMethodDetail>
    where TMethodDetail : IMethodBase<TMethodDetail>
{
    IParameterDetail[] Parameters { get; }
}
