using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotNetPowerExtensions.Reflection;

public static class TypeExtensions
{
    private static ConcurrentDictionary<Type, object?> typeDefaults = new ConcurrentDictionary<Type, object?>();
    public static object? GetDefault(this Type t)
    {
        return typeDefaults.GetOrAdd(t, t1 =>
        {
            Func<object?> f = GetDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t1).Invoke(null, null);
        });

        static T? GetDefault<T>() => default(T);
    }

    public static Models.TypeDetailInfo GetTypeDetailInfo(this Type t) => new TypeInfoService(t).GetTypeInfo();

    public static bool IsNullAllowed(this Type t) => t.GetDefault() is null;  // If type.GetDefault() is null then null is allowed, otherwise it's not...

    public static bool HasInnerType(this Type type) => type.IsGenericType || type.IsArray;

    public static Type[] GetInnerTypes(this Type type) => type.IsGenericType ? type.GenericTypeArguments :
                                                                                type.IsArray ? new[] { type.GetElementType()! } : new Type[] { };

    public static Type[] GetBaseTypes(this Type type)
    {
        var t = type;
        var i = 0;
        var list = new List<Tuple<Type, int>>();

        while (t.BaseType is not null && t.BaseType != typeof(object) && t.BaseType != t)
        {
            list.Add(Tuple.Create(t.BaseType, i++));
            t = t.BaseType;
        }

        return list.OrderByDescending(x => x.Item2).Select(x => x.Item1).ToArray();
    }

    public static IEnumerable<Type> GetBasesAndInterfaces(this Type type) => type.GetInterfaces().Union(type.GetBaseTypes());

    public static IEnumerable<Type> GetAllGenericDefinitions(this Type type)
        => type.GetBasesAndInterfaces().Union(new Type[] { type })
                .Where(t => t.IsGenericType)
                .Select(t => t.IsGenericTypeDefinition ? t : t.GetGenericTypeDefinition());

    public static bool IsDelegate(this Type type) => typeof(Delegate).IsAssignableFrom(type.GetTypeInfo().BaseType);

    /// <summary>
    /// Convenience reflection helper for the case when the method is not generic and the arguments are not null
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="instance"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static object? InvokeMethod(this Type type, string methodName, object? instance = null, object[]? args = null)
    {
        if (args is not null && args.Any(a => a is null)) throw new ArgumentNullException(nameof(args), "Null arguments not allowed");

        var method = type.GetMethod(methodName, BindingFlagsExtensions.AllBindings, null, args?.Select(p => p.GetType()).ToArray() ?? new Type[] { }, null);
        if (method is null) throw new ArgumentOutOfRangeException($"Method `{methodName}` with specified arguments of types `{string.Join(", ",args?.Select(p => p.GetType().Name) ?? new string[]{})}` not found");

        return method.Invoke(instance, args);
    }

    private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, InterfaceMapping>> interfaceMappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, InterfaceMapping>>();
    public static InterfaceMapping GetInterfaceMapForInterface(this Type type, Type ifaceType)
    {
        if (type.IsGenericParameter) throw new InvalidOperationException("Arg_GenericParameter");
        if (type is null) throw new ArgumentNullException(nameof(type));
        if (ifaceType is null) throw new ArgumentNullException(nameof(ifaceType));
        if (ifaceType.GetType().FullName != "System.RuntimeType") throw new ArgumentException("MustBeRuntimeType");
        //TypeHandle.VerifyInterfaceIsImplemented(typeHandle);
        if ((bool?)ifaceType.GetType().GetProperty("IsSZArray")?.GetValue(type) == true && ifaceType.IsGenericType)
                                                                                throw new ArgumentException("SR.Argument_ArrayGetInterfaceMap");

        return interfaceMappings.GetOrAdd(type, new ConcurrentDictionary<Type, InterfaceMapping>())
            .GetOrAdd(ifaceType, ifaceType =>
            {
                int numVirtualsAndStaticVirtuals = (int)typeof(RuntimeTypeHandle).InvokeMethod("GetNumVirtualsAndStaticVirtuals", null, new[] { ifaceType })!;

                var interfaceMethods = new List<MethodInfo>();
                var targetMethods = new List<MethodInfo>();

                for (int i = 0; i < numVirtualsAndStaticVirtuals; i++)
                {
                    var result = GetMap(i);
                    if (result is null) continue;

                    interfaceMethods.Add(result.Item1);
                    targetMethods.Add(result.Item2);
                }

                return new InterfaceMapping
                {
                    InterfaceType = ifaceType,
                    TargetType = type,
                    InterfaceMethods = interfaceMethods.Where(m => m is not null).ToArray(),
                    TargetMethods = targetMethods.Where(m => m is not null).ToArray(),
                };
            });

        Tuple<MethodInfo, MethodInfo>? GetMap(int i)
        {
            var methodAt = typeof(RuntimeTypeHandle).InvokeMethod("GetMethodAt", null, new object[] { ifaceType, i })!;

            var ifaceMethod = (MethodInfo)ifaceType.GetType().InvokeMethod("GetMethodBase", null, new object[] { ifaceType, methodAt })!;
            if (ifaceMethod is null) return null;

            var interfaceMethodImplementation = typeof(RuntimeTypeHandle)
                .InvokeMethod("GetInterfaceMethodImplementation", type.TypeHandle, new[] { ifaceType.TypeHandle, methodAt })!;
            if ((bool)typeof(Type).Assembly.GetType("System.RuntimeMethodHandleInternal")?
                                .InvokeMethod("IsNullHandle", interfaceMethodImplementation, new object[] { })!)
                return null;

            Type declaringType = (Type)typeof(RuntimeMethodHandle).InvokeMethod("GetDeclaringType", null, new object[] { interfaceMethodImplementation })!;

            var targetMethod = (MethodInfo)ifaceType.GetType()
                    .InvokeMethod("GetMethodBase", null, new object[] { declaringType.IsInterface ? declaringType : type, interfaceMethodImplementation })!;

            return targetMethod is null ? null : Tuple.Create(ifaceMethod, targetMethod);
        }
    }
}
