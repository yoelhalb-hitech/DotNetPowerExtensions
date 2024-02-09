
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IMethodBase<TMethodDetail> : IMemberDetail<TMethodDetail>
    where TMethodDetail : IMethodBase<TMethodDetail>
{
    IParameterDetail[] Parameters { get; }
}
