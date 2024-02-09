
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IMethodDetail : IMethodBase<IMethodDetail>, IGenericBase<IMethodDetail>
{
    ITypeDetailInfo ReturnType { get; }

    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    IMethodDetail? OverridenMethod { get; }
}
