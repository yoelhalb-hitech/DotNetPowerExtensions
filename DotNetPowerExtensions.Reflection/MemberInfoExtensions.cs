
namespace SequelPay.DotNetPowerExtensions.Reflection;

public static class MemberInfoExtensions
{
    public static bool IsPrivate<T>(T t) where T : MemberInfo => t switch
    {
        MethodBase m => m.IsPrivate,
        PropertyInfo p => p.IsPrivate(),
        FieldInfo f => f.IsPrivate,
        EventInfo e => e.IsPrivate(),
        Type type => type.IsNestedPrivate,
        _ => throw new NotSupportedException(typeof(T).Name),
    };

    public static bool isAbstract<T>(T t) where T : MemberInfo => t switch
    {
        MethodBase m => m.IsAbstract,
        PropertyInfo p => p.IsAbstract(),
        FieldInfo f => false,
        EventInfo e => e.IsAbstract(),
        Type type => type.IsAbstract,
        _ => throw new NotSupportedException(typeof(T).Name),
    };
}
