
namespace SequelPay.DotNetPowerExtensions;

public static class ArrayUtils
{
    public static T[] Empty<T>() =>
#if NET7_0_OR_GREATER
        Array.Empty<T>();
#else
        new T[0];
#endif
}

