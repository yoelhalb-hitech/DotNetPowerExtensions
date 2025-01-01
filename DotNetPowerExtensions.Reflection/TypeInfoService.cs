using System.Collections.Concurrent;

namespace SequelPay.DotNetPowerExtensions.Reflection;

internal class TypeInfoService
{
    private readonly Type type;

    private static ConcurrentDictionary<Type, TypeDetailInfo> types = new ConcurrentDictionary<Type, TypeDetailInfo>();

    public TypeInfoService(Type type)
    {
        this.type = type;
    }

    public static TypeDetailInfo GetTypeInfo(Type type)
        => types.GetOrAdd(type, t => new TypeDetailInfo(t));

    public TypeDetailInfo GetTypeInfo()
        => GetTypeInfo(type);
}
