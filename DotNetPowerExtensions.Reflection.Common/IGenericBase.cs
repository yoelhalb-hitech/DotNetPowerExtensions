
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IGenericBase<TDetail>
{
    ITypeDetailInfo[] GenericArguments { get; }

    bool IsGeneric { get; }
    bool IsGenericDefinition { get; }
    bool IsConstructedGeneric { get; }
    TDetail? GenericDefinition { get; }

    TDetail GetConstructedGeneric(ITypeDetailInfo[] genericArgs);
}
