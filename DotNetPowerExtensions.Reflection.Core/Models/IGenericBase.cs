
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

public interface IGenericBase<TDetail>
{
    ITypeDetailInfo[] GenericArguments { get; }

    bool IsGeneric { get; }
    bool IsGenericDefinition { get; }
    bool IsConstructedGeneric { get; }
    TDetail? GenericDefinition { get; }

    TDetail GetConstructedGeneric(ITypeDetailInfo[] genericArgs);
}
