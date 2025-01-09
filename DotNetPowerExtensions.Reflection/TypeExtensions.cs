using System.Collections.Concurrent;

namespace SequelPay.DotNetPowerExtensions.Reflection;

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

    public static TypeDetailInfo GetTypeDetailInfo(this Type t) => TypeInfoService.GetTypeInfo(t);

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
    /// Convenience reflection helper for the case when the method is not generic
    /// </summary>
    /// <param name="type"><see cref="Type"/> object of the containing class</param>
    /// <param name="methodName">The name of the method</param>
    /// <param name="instance">The object upon which to invoke the method, or null if it is a static method</param>
    /// <param name="args">Array of arguments</param>
    /// <returns>The return value of the object if any</returns>
    /// <exception cref="Exception"></exception>
    /// <remarks>This only works on a non generic</remarks>
    public static object? InvokeMethod(this Type type, string methodName, object? instance = null, object[]? args = null)
    {
        // TODO... maybe add analyzer and code fix or completion provider to help the user at compile time

        args ??= [];
        if (args.Any(a => a is null)) throw new ArgumentNullException(nameof(args), "Null arguments not allowed");

        var detail = type.GetTypeDetailInfo(); // Using this instead of Type to solve loads of issues...
        var argTypes = args.Select(a => a.GetType()).ToArray();

        Func<MethodDetail, bool> filterForExact = d => d.Name == methodName && d.Parameters.Select(p => p.ParameterType.Type).SequenceEqual(argTypes);
        Func<MethodDetail, bool> filterForAssignable = d => d.Name == methodName && d.Parameters.Length == args.Length && d.Parameters.Select((p, i) => p.ParameterType.Type.IsAssignableFrom(argTypes[i])).All(b => b);

        // TODO... maybe we should prefer an explicit method over a static method when `instance` was passed
        var methods = detail.MethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.MethodDetails.Where(filterForAssignable);
        if (!methods.Any()) methods = detail.ExplicitMethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.ExplicitMethodDetails.Where(filterForAssignable);
        if (!methods.Any()) methods = detail.BasePrivateMethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.BasePrivateMethodDetails.Where(filterForAssignable);

        var typeString = argTypes.Any() ? $"specified arguments of {(argTypes.HasOnlyOne() ? "type" : "types")} `{string.Join(", ",argTypes.Select(p => p.Name))}`" : "no arguments";
        if (methods.Empty()) throw new ArgumentOutOfRangeException($"Method `{methodName}` with {typeString} not found");
        if (methods.HasMoreThanOne()) throw new AmbiguousMatchException($"Found multiple methods `{methodName}` {typeString}");

        return methods.First().ReflectionInfo.Invoke(instance, args);
    }

    /// <summary>
    /// Convenience reflection helper for the case when the method is not generic using named parameters
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="instance"></param>
    /// <param name="namedArgs">A <see cref="Dictionary{string, object}"/> of named arguments</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static object? InvokeMethod(this Type type, string methodName, object? instance, Dictionary<string, object?> namedArgs)
    {
        // TODO... maybe add analyzer and code fix or completion provider to help the user at compile time
        var detail = type.GetTypeDetailInfo(); // Using this instead of Type to solve loads of issues...
        var argTypes = namedArgs.ToDictionary(a => a.Key, a => a.Value?.GetType()).ToArray();

        Func<MethodDetail, bool> filterForExact = d => d.Name == methodName && argTypes.All(a => d.Parameters.Any(p => p.Name == a.Key && (a.Value is null || p.ParameterType.Type == a.Value)));
        Func<MethodDetail, bool> filterForAssignable = d => d.Name == methodName && argTypes.All(a => d.Parameters.Any(p => p.Name == a.Key && (a.Value is null || p.ParameterType.Type.IsAssignableFrom(a.Value))));

        // TODO... maybe we should prefer an explicit method over a static method when `instance` was passed
        var methods = detail.MethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.MethodDetails.Where(filterForAssignable);
        if (!methods.Any()) methods = detail.ExplicitMethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.ExplicitMethodDetails.Where(filterForAssignable);
        if (!methods.Any()) methods = detail.BasePrivateMethodDetails.Where(filterForExact);
        if (!methods.Any()) methods = detail.BasePrivateMethodDetails.Where(filterForAssignable);

        var typeString = argTypes.Any() ? $"specified arguments of `{string.Join(", ", argTypes.Select(p => p.Key + (p.Value is Type t ? ":" + t.Name : "")))}`" : "no arguments";
        if (methods.Empty()) throw new ArgumentOutOfRangeException($"Method `{methodName}` with {typeString} not found");
        if (methods.HasMoreThanOne() && methods.Any(m => m.Parameters.Length == namedArgs.Count)) methods = methods.Where(m => m.Parameters.Length == namedArgs.Count);
        if (methods.HasMoreThanOne()) throw new AmbiguousMatchException($"Found multiple methods `{methodName}` {typeString}");

        var method = methods.First();
        var args = method.Parameters.Select(p => namedArgs.ContainsKey(p.Name) ? namedArgs[p.Name] : p.ParameterType.Type.GetDefault()).ToArray();
        return method.ReflectionInfo.Invoke(instance, args);
    }

    private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, InterfaceMapping>> interfaceMappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, InterfaceMapping>>();
    public static InterfaceMapping GetInterfaceMapForInterface(this Type type, Type ifaceType)
    {
        if (!type.IsInterface) return type.GetInterfaceMap(ifaceType);

        if (type.IsGenericParameter) throw new InvalidOperationException("Arg_GenericParameter");
        if (type is null) throw new ArgumentNullException(nameof(type));
        if (ifaceType is null) throw new ArgumentNullException(nameof(ifaceType));
        if (ifaceType.GetType().FullName != "System.RuntimeType") throw new ArgumentException("MustBeRuntimeType");

        // SZArrays implement the methods on IList`1, IEnumerable`1, and ICollection`1 with
        //      SZArrayHelper and some runtime magic. We don't have accurate interface maps for them.
        if ((bool?)type.GetType().GetProperty("IsSZArray")?.GetValue(type) == true && ifaceType.IsGenericType)
                                        throw new ArgumentException("Interface maps for generic interfaces on arrays cannot be retrieved."); // "SR.Argument_ArrayGetInterfaceMap"

        return interfaceMappings.GetOrAdd(type, new ConcurrentDictionary<Type, InterfaceMapping>())
            .GetOrAdd(ifaceType, ifaceType =>
            {
                var interfaceMethods = new List<MethodInfo>();
                var targetMethods = new List<MethodInfo>();

                if (type.GetType().Assembly.GetName().Name != "mscorlib") // .Net Framework doesn't support Default Interface Methods, and also not `GetNumVirtualsAndStaticVirtuals`
                {
                    int numVirtualsAndStaticVirtuals = (int)typeof(RuntimeTypeHandle).InvokeMethod("GetNumVirtualsAndStaticVirtuals", null, new[] { ifaceType })!;

                    for (int i = 0; i < numVirtualsAndStaticVirtuals; i++)
                    {
                        var result = GetMap(i);
                        if (result is null) continue;

                        interfaceMethods.Add(result.Item1);
                        targetMethods.Add(result.Item2);
                    }
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
