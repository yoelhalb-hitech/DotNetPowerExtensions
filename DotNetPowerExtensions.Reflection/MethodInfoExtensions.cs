using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetPowerExtensions.Reflection;

public static class MethodInfoExtensions
{
    /// <summary>
    /// If the original method is the equivalent of <see langword="internal" /> or <see langword="internal protected" />
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public static bool IsInternal(this MethodInfo methodInfo) => methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly;

    public static bool IsPublicOrInternal(this MethodInfo methodInfo) => methodInfo.IsPublic || methodInfo.IsInternal();

    public static bool IsOverridable(this MethodInfo methodInfo) => !methodInfo.IsPrivate && methodInfo.IsVirtual && !methodInfo.IsFinal; //https://learn.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isfinal#System_Reflection_MethodBase_IsFinal

    public static bool IsVoid(this MethodInfo method) => method.ReturnType == typeof(void);

    private static ConcurrentDictionary<Type, Dictionary<MethodInfo, PropertyInfo>> propertyMethodCache = new ConcurrentDictionary<Type, Dictionary<MethodInfo, PropertyInfo>>();
    public static PropertyInfo? GetDeclaringProperty(this MethodInfo methodInfo)
    {
        if(methodInfo.DeclaringType is null) throw new ArgumentOutOfRangeException(nameof(methodInfo), "Method does not have a declaring type");

        var dict = propertyMethodCache.GetOrAdd(methodInfo.DeclaringType, type =>
                                                            type.GetProperties(BindingFlagsExtensions.AllBindings | BindingFlags.DeclaredOnly) // DeclaredOnly to avoid shadowed
                                                                .SelectMany(p => p.GetAllMethods().Select(m => new { p, m }))
                                                                .ToDictionary(x => x.m, x => x.p, new MethodEqualityComparer()));
        dict.TryGetValue(methodInfo, out var result);
        return result;
    }

    private static ConcurrentDictionary<Type, Dictionary<MethodInfo, EventInfo>> eventMethodCache = new ConcurrentDictionary<Type, Dictionary<MethodInfo, EventInfo>>();
    public static EventInfo? GetDeclaringEvent(this MethodInfo methodInfo)
    {
        if (methodInfo.DeclaringType is null) throw new ArgumentOutOfRangeException(nameof(methodInfo), "Method does not have a declaring type");

        var dict = eventMethodCache.GetOrAdd(methodInfo.DeclaringType, type =>
                                                            type.GetEvents(BindingFlagsExtensions.AllBindings | BindingFlags.DeclaredOnly) // DeclaredOnly to avoid shadowed
                                                                .SelectMany(p => p.GetAllMethods().Select(m => new { p, m }))
                                                                .ToDictionary(x => x.m, x => x.p, new MethodEqualityComparer()));
        dict.TryGetValue(methodInfo, out var result);
        return result;
    }

    public static bool IsExplicitImplementation(this MethodBase method)
        => method.Name.Contains('.') && !method.IsStatic && method.IsVirtual && method.IsPrivate;

    /// <summary>
    /// In Reflection the same method will not be considered equal if it was obtained from a different subclass, even if the the subclass didn't override the method
    /// This method will correctly handle it
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool IsEqual(this MethodInfo methodInfo, MethodInfo other)
    {
        if(methodInfo is null) throw new ArgumentNullException(nameof(methodInfo));
        if(other is null) throw new ArgumentNullException(nameof(other));

        // Based on my answer at https://stackoverflow.com/a/76049509/640195

        if (methodInfo.ReflectedType == other.ReflectedType) return methodInfo == other;
        if(methodInfo.DeclaringType != other.DeclaringType) return false;

        return methodInfo.IsSignatureEqual(other);
    }

    public class MethodEqualityComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals(MethodInfo? x, MethodInfo? y) => x is not null && y is not null && x.IsEqual(y);

        public int GetHashCode(MethodInfo m) => m.Name.GetHashCode() * m.DeclaringType.GetHashCode();
    }

    public static bool IsSignatureEqual(this MethodInfo methodInfo, MethodInfo other)
        => methodInfo.Name == other.Name
                && methodInfo.GetParameters().Select(p => p.ParameterType).SequenceEqual(other.GetParameters().Select(p => p.ParameterType))
                && methodInfo.GetGenericArguments().SequenceEqual(other.GetGenericArguments());

    public static IEnumerable<MethodInfo> GetInterfaceMethods(this MethodInfo method)
    {
        if (method.ReflectedType!.IsInterface && !method.IsExplicitImplementation()) yield break; // An interface doesn't implement another interface unless explicit

        // Using ReflectedType since a method/property in a base class (which doesn't implement) can also be used as the interface implementation in the subclass
        var ifaces = method.ReflectedType.GetInterfaces();
        foreach (var iface in ifaces)
        {
            var map = method.ReflectedType.GetInterfaceMapForInterface(iface);
            for (int i = 0; i < map.TargetMethods.Length; i++)
            {
                if (map.TargetMethods[i] == method) yield return map.InterfaceMethods[i];
            }
        }

        yield break;
    }
}
