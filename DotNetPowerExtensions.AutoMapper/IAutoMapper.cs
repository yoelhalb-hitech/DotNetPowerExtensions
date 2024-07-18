
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

public interface IAutoMapper
{
    TTarget Map<TSource, TTarget>(TSource source) where TTarget : new();
}
