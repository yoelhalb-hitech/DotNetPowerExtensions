
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

public static class Mapper
{
    public static TTarget AutoMap<TSource, TTarget>(TSource source) where TTarget : new()
        => new AutoMapper().Map<TSource, TTarget>(source);
}
